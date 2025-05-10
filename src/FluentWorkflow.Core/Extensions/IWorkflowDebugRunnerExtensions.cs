#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FluentWorkflow.MessageDispatch;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IWorkflowDebugRunnerExtensions
{
    #region Public 方法

    /// <summary>
    /// 开始执行消息对应的流程（仅当 <paramref name="transmissionModelRawData"/> 为 Json 结构时支持调用此方法）
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="transmissionModelRawData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Task RunAsync(this IWorkflowDebugRunner runner, ReadOnlyMemory<byte> transmissionModelRawData, CancellationToken cancellationToken = default)
    {
        var jsonObject = JsonNode.Parse(utf8Json: transmissionModelRawData.Span,
                                        nodeOptions: new JsonNodeOptions() { PropertyNameCaseInsensitive = true },
                                        documentOptions: new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip }) as JsonObject
                         ?? throw new ArgumentException("Can not parse the input data");
        if (!jsonObject.TryGetPropertyValue(nameof(IDataTransmissionModel<object>.EventName), out var eventNameNode)
            || eventNameNode?.GetValue<string>() is not string eventName
            || string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Can not get event name in the input data");
        }

        return runner.RunAsync(eventName, transmissionModelRawData, cancellationToken);
    }

    #endregion Public 方法
}
