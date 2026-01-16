using MessagePack;

namespace FluentWorkflow.Management.Shared.Messages;

/// <summary>
/// Ping
/// </summary>
[MessagePackObject]
public record Ping
{
}

/// <summary>
/// Pong
/// </summary>
[MessagePackObject]
public record Pong
{
}
