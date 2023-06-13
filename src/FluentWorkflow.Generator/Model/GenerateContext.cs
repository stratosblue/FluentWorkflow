using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentWorkflow.Generator.Model;

internal class GenerateContext
{
    #region Public 属性

    public GenerateNames GenerateNames { get; }

    public ImmutableArray<StageName> Stages { get; }

    public WorkflowDescriptor WorkflowDescriptor { get; }

    public string Usings { get; }

    #endregion Public 属性

    #region Public 构造函数

    public GenerateContext(WorkflowDescriptor workflowDescriptor)
    {
        WorkflowDescriptor = workflowDescriptor;
        GenerateNames = new GenerateNames(workflowDescriptor);

        var stagesAnalyzer = new WorkflowStagesAnalyzer(GenerateNames);
        workflowDescriptor.DeclarationSyntax.Accept(stagesAnalyzer);

        Stages = stagesAnalyzer.Stages;

        var usingBuilder = new StringBuilder(512);
        foreach (var usingItem in WorkflowDescriptor.DeclarationSyntax.SyntaxTree.GetCompilationUnitRoot().Usings)
        {
            usingBuilder.AppendLine(usingItem.ToString());
        }

        usingBuilder.AppendLine($"using {GenerateNames.NameSpace};");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.Continuator;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.Message;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.Internal;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.Handler;");

        Usings = usingBuilder.ToString();
    }

    #endregion Public 构造函数
}
