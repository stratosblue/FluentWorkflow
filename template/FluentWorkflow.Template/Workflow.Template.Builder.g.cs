﻿// <Auto-Generated/>
using System.ComponentModel;
using FluentWorkflow;

namespace TemplateNamespace.Template.Internal;

/// <summary>
/// <see cref="TemplateWorkflow"/> 构造器基类
/// </summary>
/// <typeparam name="TWorkflow"><see cref="TemplateWorkflow"/> 或其派生类型</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
internal abstract partial class TemplateWorkflowBuilderBase<TWorkflow>
    : SimpleWorkflowBuilder<TWorkflow, TemplateWorkflowContext, ITemplateWorkflow>
    where TWorkflow : TemplateWorkflow
{
    /// <inheritdoc cref="TemplateWorkflowBuilderBase{TWorkflow}"/>
    protected TemplateWorkflowBuilderBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override TemplateWorkflowContext BuildContext(IEnumerable<KeyValuePair<string, string>> context)
    {
        return new TemplateWorkflowContext(context);
    }
}

/// <summary>
/// <see cref="TemplateWorkflow"/> 构造器
/// </summary>
/// <typeparam name="TWorkflow"><see cref="TemplateWorkflow"/> 或其派生类型</typeparam>
internal partial class TemplateWorkflowBuilder<TWorkflow>
    : TemplateWorkflowBuilderBase<TWorkflow>
    where TWorkflow : TemplateWorkflow
{
    /// <inheritdoc cref="TemplateWorkflowBuilder{TWorkflow}"/>
    public TemplateWorkflowBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
