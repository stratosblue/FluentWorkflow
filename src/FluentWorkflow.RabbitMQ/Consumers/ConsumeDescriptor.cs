using FluentWorkflow.Build;

namespace FluentWorkflow.RabbitMQ;

internal record class ConsumeDescriptor(string EventName, WorkflowEventInvokerDescriptor[] InvokerDescriptors, MessageRequeuePolicy RequeuePolicy, TimeSpan RequeueDelay);
