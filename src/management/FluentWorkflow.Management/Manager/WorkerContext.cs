using FluentWorkflow.Management.Shared.Features;
using Hoarwell;

namespace FluentWorkflow.Management.Manager;

/// <summary>
/// 工作者上下文
/// </summary>
/// <param name="Context">连接上下文</param>
/// <param name="Descriptor">工作者描述符</param>
public record WorkerContext(IHoarwellContext Context, WorkerDescriptor Descriptor)
{
    /// <summary>
    /// 通信管道
    /// </summary>
    public required ICommunicationPipeFeature CommunicationPipe { get; init; }

    /// <summary>
    /// 工作者Id
    /// </summary>
    public Guid Id => Descriptor.Id;

    /// <summary>
    /// 工作者名称
    /// </summary>
    public string Name => Descriptor.Name;

    /// <summary>
    /// 工作者运行Token
    /// </summary>
    public CancellationToken CancellationToken { get; } = Context.ExecutionAborted;

    /// <summary>
    /// 是否活跃状态
    /// </summary>
    public bool IsActive { get; internal set; } = true;
}
