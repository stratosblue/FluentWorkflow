using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// 客户端发起接入请求
/// </summary>
[MessagePackObject]
public record Hello
{
    /// <summary>
    /// Id
    /// </summary>
    [Key(0)]
    public required Guid Id { get; set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    [Key(1)]
    public required int ProtocolVersion { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Key(2)]
    public required string WhoIs { get; set; }

    /// <summary>
    /// 主机名
    /// </summary>
    [Key(3)]
    public required string HostName { get; set; }

    /// <summary>
    /// 用于认证的Cookie
    /// </summary>
    [Key(4)]
    public required string Cookie { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    [Key(5)]
    public required string Version { get; set; }

    /// <summary>
    /// 启动时间
    /// </summary>
    [Key(6)]
    public required DateTime StartupTime { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [Key(7)]
    public required IDictionary<string, string> Metadata { get; set; }
}
