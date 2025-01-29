using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentWorkflow.Generator.Model;

internal class GenerateContext
{
    #region Public 属性

    public GenerateNames GenerateNames { get; }

    public ImmutableArray<StageName> Stages { get; }

    public WorkflowDeclaration WorkflowDeclaration { get; }

    public string Usings { get; }

    #endregion Public 属性

    #region Public 构造函数

    public GenerateContext(WorkflowDeclaration workflowDeclaration)
    {
        WorkflowDeclaration = workflowDeclaration;
        GenerateNames = new GenerateNames(workflowDeclaration);
        Stages = workflowDeclaration.Stages.Select(name => new StageName(name, $"{GenerateNames.WorkflowDeclaration.WorkflowClassName}{name}Stage")).ToImmutableArray();

        var usingBuilder = new StringBuilder(512);
        foreach (var usingItem in WorkflowDeclaration.DeclarationSyntax.SyntaxTree.GetCompilationUnitRoot().Usings)
        {
            usingBuilder.AppendLine(usingItem.ToString());
        }

        usingBuilder.AppendLine($"using {GenerateNames.NameSpace};");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.{GenerateNames.WorkflowName};");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.{GenerateNames.WorkflowName}.Continuator;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.{GenerateNames.WorkflowName}.Message;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.{GenerateNames.WorkflowName}.Internal;");
        usingBuilder.AppendLine($"using {GenerateNames.NameSpace}.{GenerateNames.WorkflowName}.Handler;");

        Usings = usingBuilder.ToString();
    }

    #endregion Public 构造函数
}
