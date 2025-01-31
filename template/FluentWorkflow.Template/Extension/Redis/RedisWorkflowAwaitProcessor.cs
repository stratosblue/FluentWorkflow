using System.ComponentModel;
using FluentWorkflow.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FluentWorkflow.GenericExtension.TemplateNamespace;

/// <summary>
/// 基于 Redis 的 <inheritdoc cref="IWorkflowAwaitProcessor"/>
/// </summary>
internal sealed class RedisWorkflowAwaitProcessor : IWorkflowAwaitProcessor
{
    #region Private 字段

    #region const

    private const string AllFinishedCountKey = "__AllFinishedCount";

    private const string FirstFailureKey = "__FirstFailure";

    #endregion const

    #region Script

    //工作逻辑为为该流程的key添加键 __AllFinishedCount，值为成功完成时该流程的key应该存在的字段数量（子流程数量+1）
    //当流程失败时，会添加键 __FirstFailure 记录失败的流程，此时该流程的key对应的字段数量永远大于设定的 __AllFinishedCount

    /// <summary>
    /// 当前子工作流程致命失败时的脚本，用以尽可能保证不重入的情况下，只触发一次完成事件
    /// </summary>
    private const string FatalFailureFinishScript =
        $$"""
        local key = KEYS[1]
        redis.call('HDEL', key, KEYS[2])
        return redis.call('HSETNX',key,'{{FirstFailureKey}}',KEYS[3])
        """;

    /// <summary>
    /// 当前子工作流程成功完成时的脚本，用以尽可能保证不重入的情况下，只触发一次完成事件
    /// </summary>
    private const string SuccessFinishScript =
        $$"""
        local key = KEYS[1]
        redis.call('HDEL', key, KEYS[2])
        return {redis.call('HLEN',key),redis.call('HGET',key,'{{AllFinishedCountKey}}')}
        """;

    #endregion Script

    /// <summary>
    /// 全部完成计数
    /// </summary>
    private static readonly RedisValue s_allFinishedCountKey = new(AllFinishedCountKey);

    private static readonly RedisValue s_emptyValue = new(string.Empty);

    /// <summary>
    /// 第一个失败的子工作流程
    /// </summary>
    private static readonly RedisValue s_firstFailureKey = new(FirstFailureKey);

    private readonly IDatabase _database;

    private readonly TimeSpan _expireDelay;

    private readonly string _keyPrefix;

    private readonly ILogger _logger;

    private readonly IObjectSerializer _objectSerializer;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="RedisWorkflowAwaitProcessor"/>
    public RedisWorkflowAwaitProcessor(IOptions<RedisWorkflowAwaitProcessorOptions> optionsAccessor,
                                       IFluentWorkflowRedisConnectionProvider redisConnectionProvider,
                                       IObjectSerializer objectSerializer,
                                       ILogger<RedisWorkflowAwaitProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);
        ArgumentNullException.ThrowIfNull(redisConnectionProvider);
        ArgumentNullException.ThrowIfNull(objectSerializer);

        _keyPrefix = optionsAccessor.Value.KeyPrefix ?? string.Empty;
        _expireDelay = optionsAccessor.Value.FinishedItemExpireDelay;

        _database = redisConnectionProvider.Get().GetDatabase();
        _objectSerializer = objectSerializer;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public async Task<WorkflowAwaitState> FinishedOneAsync(IWorkflowFinishedMessage finishedMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finishedMessage);

        var context = finishedMessage.Context;
        if (context.Parent is not { } parentContextSnapshot)
        {
            throw new WorkflowInvalidOperationException($"Context - \"{context.Id}\" has no parent.");
        }

        _logger.LogDebug("Workflow [{ParentWorkflowId}]'s child workflow [{ChildWorkflowId}] finished with [{IsSuccess}].", parentContextSnapshot.Id, finishedMessage.Id, finishedMessage.IsSuccess);

        var primaryKey = PrimaryKey(parentContextSnapshot.Id);

        if (!await _database.KeyExistsAsync(primaryKey))    //不存在此任务的Key
        {
            throw new WorkflowInvalidOperationException($"The context data for \"{primaryKey}\" no existed.");
        }

        var currentAlias = finishedMessage.Context.State.Alias ?? throw new InvalidOperationException("The context has not alias.");
        var currentMarkField = AliasMarkField(currentAlias);

        //设置当前完成工作流程的上下文
        await _database.HashSetAsync(primaryKey, currentAlias, PureCurrentContext(context));

        var isCurrentFatalFailure = !finishedMessage.IsSuccess
                                    && !finishedMessage.Context.Flag.HasFlag(WorkflowFlag.NotCareFinishState);

        if (!isCurrentFatalFailure) //当前子工作流程成功
        {
            //检查完成状态
            var executeResult = (int[])(await _database.ScriptEvaluateAsync(SuccessFinishScript, new RedisKey[] { primaryKey, currentMarkField }))!;

            //已结束(键数量等于完成时应有数量)
            if (executeResult[0] == executeResult[1])
            {
                _logger.LogDebug("Workflow [{ParentWorkflowId}]'s child workflow [{ChildWorkflowId}] finished. The workflow all finished.", parentContextSnapshot.Id, finishedMessage.Id);

                ExpireFinishedItemAsync();

                return await CreateFinished();
            }
        }
        else    //当前子工作流程致命失败
        {
            var firstFailureAlias = (string?)(await _database.HashGetAsync(primaryKey, s_firstFailureKey));
            if (string.IsNullOrEmpty(firstFailureAlias))    //在此之前没有失败的
            {
                ExpireFinishedItemAsync();

                //检查完成状态
                var executeResult = (int[])(await _database.ScriptEvaluateAsync(FatalFailureFinishScript, new RedisKey[] { primaryKey, currentMarkField, currentAlias }))!;
                if (executeResult[0] == 1)  //当前设置成功
                {
                    return await CreateFinished();
                }
            }
            else if (string.Equals(currentAlias, firstFailureAlias, StringComparison.Ordinal))   //在此之前有失败的，且为当前流程，认为是重入，可以再次触发
            {
                return await CreateFinished();
            }
            else    //在此之前有失败的，且为当前流程，认为已经被触发，不再触发
            {
                _ = _database.HashDeleteAsync(primaryKey, currentMarkField, CommandFlags.FireAndForget);
                return CreateUnFinished();
            }
        }

        return CreateUnFinished();

        #region Result

        //创建已完成的返回值
        async Task<WorkflowAwaitState> CreateFinished()
        {
            var childWorkflowContexts = await GetAllChildContexts(primaryKey);
            return new WorkflowAwaitState(parentContextSnapshot, true, childWorkflowContexts);
        }

        //创建未完成的返回值
        WorkflowAwaitState CreateUnFinished()
        {
            return new WorkflowAwaitState(parentContextSnapshot, false, new Dictionary<string, IWorkflowContext>() { { currentAlias, context } }!);
        }

        //过期已完成的条目
        void ExpireFinishedItemAsync()
        {
            _ = _database.KeyExpireAsync(primaryKey, _expireDelay, ExpireWhen.Always, CommandFlags.FireAndForget);
        }

        #endregion Result
    }

    /// <inheritdoc/>
    public async Task RegisterAsync(IWorkflowContext parentWorkflowContext, IDictionary<string, IWorkflow> childWorkflows, CancellationToken cancellationToken = default)
    {
        // 追加的字段数量
        const int AppendFieldsCount = 1;

        var hashFields = new HashEntry[childWorkflows.Count * 2 + AppendFieldsCount];

        var index = 0;
        hashFields[index++] = new HashEntry(s_allFinishedCountKey, childWorkflows.Count + AppendFieldsCount);

        foreach (var (alias, workflow) in childWorkflows)
        {
            hashFields[index++] = new HashEntry(alias, s_emptyValue);
            hashFields[index++] = new HashEntry(AliasMarkField(alias), s_emptyValue);
        }

        _logger.LogDebug("Register [{ChildWorkflowCount}] for workflow [{ParentWorkflowId}].", childWorkflows.Count, parentWorkflowContext.Id);

        await _database.HashSetAsync(PrimaryKey(parentWorkflowContext.Id), hashFields);
    }

    #endregion Public 方法

    #region Private 方法

    private string PureCurrentContext(IWorkflowContext context)
    {
        var pureCurrentContext = new Dictionary<string, string>(context.GetSnapshot());
        pureCurrentContext.Remove(FluentWorkflowConstants.ContextKeys.ParentWorkflow);
        return _objectSerializer.Serialize(pureCurrentContext);
    }

    private async Task<Dictionary<string, IWorkflowContext?>> GetAllChildContexts(RedisKey primaryKey)
    {
        var hashEntries = await _database.HashGetAllAsync(primaryKey);

        var childWorkflowContexts = new Dictionary<string, IWorkflowContext?>();

        for (int i = 0; i < hashEntries.Length; i++)
        {
            (string alias, string value) = hashEntries[i];

            if (alias != AllFinishedCountKey
                && alias != FirstFailureKey)
            {
                if (string.IsNullOrEmpty(value))
                {
                    childWorkflowContexts.Add(alias, null);
                    continue;
                }
                var childContext = string.IsNullOrEmpty(value)
                                   ? null
                                   : _objectSerializer.Deserialize<Dictionary<string, string>>(value) is { } contextRawData
                                     ? new WorkflowContextSnapshot(contextRawData!)
                                     : null;

                childWorkflowContexts.Add(alias, childContext);
            }
        }

        return childWorkflowContexts;
    }

    #region key

    private string AliasMarkField(string alias) => $"MK_{alias}";

    private RedisKey PrimaryKey(string id) => $"{_keyPrefix}FWF:{id}";

    #endregion key

    #endregion Private 方法
}

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class HashEntryExtensions
{
    #region Public 方法

    /// <summary>
    ///
    /// </summary>
    /// <param name="hashEntry"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this HashEntry hashEntry, out string name, out string value)
    {
        name = hashEntry.Name!;
        value = hashEntry.Value!.IsNullOrEmpty
                ? string.Empty
                : hashEntry.Value!;
    }

    #endregion Public 方法
}
