using System.ComponentModel;
using FluentWorkflow.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow.Util;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class FluentWorkflowOptionsContinuatorRegister<TWorkflowContinuator>
    : FluentWorkflowOptionsPostConfigure
    where TWorkflowContinuator : IWorkflowContinuator, IWorkflowNameDeclaration, IWorkflowStageNameDeclaration
{
    #region Protected 方法

    protected override void PostConfigure(FluentWorkflowOptions options)
    {
        options.Continuators.Add<TWorkflowContinuator>();
    }

    #endregion Protected 方法
}
