using System.ComponentModel.RuntimeValidation;
using FluentWorkflow.Management.Shared.MessageHandler;
using FluentWorkflow.Management.Shared.Messages;
using FluentWorkflow.MessageDispatch.DispatchControl;
using Hoarwell;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Management.Worker.MessageHandler;

internal class ConsumptionControlMessageHandler([ServiceKey] string appName, IWorkingController workingController, IServiceProvider serviceProvider)
    : GenericResponseMessageHandler<ConsumptionControl, ConsumptionControlResult>(appName, serviceProvider)
{
    #region Protected 方法

    protected override Task<ConsumptionControlResult> ProcessRequestAsync(IHoarwellContext context, ConsumptionControl input, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        if (workingController.WorkingItems.TryGetValue(input.TargetMessageId.Required(), out var workingItem))
        {
            switch (input.Type)
            {
                case ConsumptionControlType.AbortWorkflow:
                    workingItem.ControlScope.AbortWorkflow(input.Reason);
                    isSuccess = true;
                    break;

                case ConsumptionControlType.EvictRunningWork:
                    workingItem.ControlScope.EvictRunningWork(input.Reason);
                    isSuccess = true;
                    break;

                default:
                    throw new ArgumentException($"Unsupported control type: {input.Type}");
            }
        }

        var result = new ConsumptionControlResult() { IsSuccess = isSuccess };
        return Task.FromResult(result);
    }

    #endregion Protected 方法
}
