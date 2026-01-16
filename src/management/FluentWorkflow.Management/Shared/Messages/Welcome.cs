using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 服务端响应连接成功
/// </summary>
[MessagePackObject]
public record Welcome
{
    /// <summary>
    /// 声明服务端版本
    /// </summary>
    [Key(0)]
    public required string Version { get; set; }
}
