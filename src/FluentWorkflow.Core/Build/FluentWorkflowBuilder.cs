using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentWorkflow.Build;

internal class FluentWorkflowBuilder : IFluentWorkflowBuilder
{
    #region Public 属性

    public Dictionary<object, object?> Properties { get; } = new();

    public IServiceCollection Services { get; }

    public WorkflowBuildStateCollection WorkflowBuildStates { get; }

    #endregion Public 属性

    #region Public 构造函数

    public FluentWorkflowBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        if (Services.FirstOrDefault(static m => m.ServiceType == typeof(WorkflowBuildStateCollection)) is not { } existedDescriptor
            || existedDescriptor.ImplementationInstance is not WorkflowBuildStateCollection stateCollection)
        {
            stateCollection = new WorkflowBuildStateCollection();
            Services.Replace(ServiceDescriptor.Singleton<WorkflowBuildStateCollection>(stateCollection));
        }
        WorkflowBuildStates = stateCollection;
    }

    #endregion Public 构造函数
}
