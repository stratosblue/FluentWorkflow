using FluentWorkflow.MessageDispatch.DispatchControl;
using FluentWorkflow.MessageDispatch.DispatchControl.Internal;

namespace Microsoft.Extensions.DependencyInjection;

internal static class DispatchControlDIExtensions
{
    public static IWorkingController GetWorkingController(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<IWorkingController>() ?? NullWorkingController.Shared;
    }
}
