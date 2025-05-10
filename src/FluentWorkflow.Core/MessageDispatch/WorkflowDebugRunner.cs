using System.Collections.Immutable;
using System.Diagnostics;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using FluentWorkflow.Tracing;
using FluentWorkflow.Util;

namespace FluentWorkflow.MessageDispatch;

internal sealed class WorkflowDebugRunner(IMessageConsumeDispatcher messageConsumeDispatcher, WorkflowBuildStateCollection workflowBuildStates, IObjectSerializer objectSerializer)
    : IWorkflowDebugRunner
{
    #region Private 字段

    /// <summary>
    /// 工作流程事件执行程序描述符订阅列表
    /// </summary>
    private readonly ImmutableDictionary<string, ImmutableArray<WorkflowEventInvokerDescriptor>> _eventSubscribeDescriptors = workflowBuildStates.GetEventInvokeMap();

    #endregion Private 字段

    #region Public 方法

    public Task RunAsync(string eventName, ReadOnlyMemory<byte> transmissionModelRawData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventName);

        if (!_eventSubscribeDescriptors.TryGetValue(eventName, out var descriptors))
        {
            throw new ArgumentException($"No subscribe info for \"{eventName}\"");
        }
        var transmissionTypes = descriptors.Select(static m => m.TransmissionType).Distinct().ToList();
        if (transmissionTypes.Count != 1)
        {
            throw new ArgumentException($"Subscribe info for \"{eventName}\" has invalid transmission type description count: {transmissionTypes.Count}");
        }
        var transmissionType = transmissionTypes[0];

        var dataTransmissionModel = objectSerializer.Deserialize(transmissionModelRawData.Span, transmissionType) as IDataTransmissionModel<object>
                                    ?? throw new InvalidDataException($"Deserialize message \"{eventName}\" [{transmissionType}] failed.");

        if (!eventName.Equals(dataTransmissionModel.EventName, StringComparison.Ordinal))
        {
            throw new ArgumentException($"The input data has event name \"{dataTransmissionModel.EventName}\" not match input event name \"{eventName}\"");
        }

        return InnerRunAsync(eventName, dataTransmissionModel, cancellationToken);
    }

    public Task RunAsync<TMessage>(DataTransmissionModel<TMessage> transmissionModel, CancellationToken cancellationToken = default)
        where TMessage : IWorkflowMessage, IEventNameDeclaration
    {
        ArgumentNullException.ThrowIfNull(transmissionModel);

        if (!_eventSubscribeDescriptors.TryGetValue(TMessage.EventName, out var descriptors)
            || descriptors.IsDefaultOrEmpty)
        {
            throw new ArgumentException($"No subscribe info for \"{TMessage.EventName}\"");
        }

        return InnerRunAsync(TMessage.EventName, (IDataTransmissionModel<object>)transmissionModel, cancellationToken);
    }

    #endregion Public 方法

    #region Private 方法

    private async Task InnerRunAsync(string eventName, IDataTransmissionModel<object> transmissionModel, CancellationToken cancellationToken = default)
    {
        Activity? activity = null;
        try
        {
            if (transmissionModel.TracingContext is { } tracingContext)
            {
                var activityContext = tracingContext.RestoreActivityContext(true);

                activity = FluentWorkflowDebugUtil.DebugActivitySource.StartActivity($"Debug WorkflowEventMessage {eventName}", ActivityKind.Internal, activityContext);

                activity?.AddBaggages(tracingContext.Baggage);
            }

            await messageConsumeDispatcher.DispatchAsync(eventName, transmissionModel.Message, cancellationToken);
        }
        finally
        {
            activity?.Dispose();
        }
    }

    #endregion Private 方法
}
