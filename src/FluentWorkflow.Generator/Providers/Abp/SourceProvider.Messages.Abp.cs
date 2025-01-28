﻿using System.Text;
using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal class AbpMessagesSourceProvider : WorkflowSourceProvider
{
    #region Public 构造函数

    public AbpMessagesSourceProvider(GenerateContext generateContext) : base(generateContext)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IEnumerable<GeneratedSource?>? Generate()
    {
        var builder = new StringBuilder(1024);

        builder.AppendLine($@"{FluentWorkflowGeneratorConstants.CodeHeader}

{Context.Usings}
using Volo.Abp.EventBus;

namespace {NameSpace}.Message;

[EventName(EventName)]
partial class {WorkflowClassName}StartRequestMessage {{ }}

[EventName(EventName)]
partial class {WorkflowClassName}FinishedMessage {{ }}

[EventName(EventName)]
partial class {WorkflowClassName}FailureMessage {{ }}
");
        foreach (var stage in Context.Stages)
        {
            builder.AppendLine($@"
[EventName(EventName)]
partial class {WorkflowClassName}{stage.Name}StageMessage {{ }}

[EventName(EventName)]
partial class {WorkflowClassName}{stage.Name}StageCompletedMessage {{ }}
");
        }

        yield return new($"{WorkflowClassName}.Messages.Abp.g.cs", builder.ToString());
    }

    #endregion Public 方法
}
