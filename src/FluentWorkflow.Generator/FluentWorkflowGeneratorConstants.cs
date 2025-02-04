﻿namespace FluentWorkflow.Generator;

internal static class FluentWorkflowGeneratorConstants
{
    public const string CodeHeader = @"// <Auto-Generated/>

#nullable enable

#pragma warning disable CS0105
#pragma warning disable IDE0130

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using FluentWorkflow;
using FluentWorkflow.Abstractions;
using FluentWorkflow.Build;
using FluentWorkflow.Diagnostics;
using FluentWorkflow.Extensions;
using FluentWorkflow.Handler;
using FluentWorkflow.MessageDispatch;
using FluentWorkflow.Scheduler;
using FluentWorkflow.Util;
";

    public static class CodeNames
    {
        public const string PropertyDefineMethod = "Property";
        public const string WorkflowName = "Name";
        public const string StageBuilderBegin = "Begin";
        public const string StageBuilderThen = "Then";
        public const string StageBuilderCompletion = "Completion";
    }
}
