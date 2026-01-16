using System.Net;

namespace FluentWorkflow.Management.Manager;

/// <summary>
/// 工作者描述符
/// </summary>
/// <param name="Id">Id</param>
/// <param name="Name">名称</param>
/// <param name="HostName">主机名称</param>
/// <param name="Version">版本号</param>
/// <param name="StartupTime">启动时间</param>
/// <param name="Metadata">元数据字典</param>
/// <param name="RemoteEndPoint">远程端点</param>
public record WorkerDescriptor(Guid Id, string Name, string HostName, string Version, DateTime StartupTime, IDictionary<string, string> Metadata, EndPoint? RemoteEndPoint)
{
    /// <summary>
    /// 处理中数量
    /// </summary>
    public int ProcessingCount { get; set; }
}
