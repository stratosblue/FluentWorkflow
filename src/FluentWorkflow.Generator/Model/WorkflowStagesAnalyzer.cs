using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator.Model;

internal class WorkflowStagesAnalyzer : CSharpSyntaxWalker
{
    #region Private 字段

    private readonly GenerateNames _generateNames;

    private readonly List<StageName> _stages = new();

    #endregion Private 字段

    #region Public 属性

    public ImmutableArray<StageName> Stages => ImmutableArray.Create((_stages as IEnumerable<StageName>).Reverse().ToArray());

    #endregion Public 属性

    #region Public 构造函数

    public WorkflowStagesAnalyzer(GenerateNames generateNames)
    {
        _generateNames = generateNames;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        //TODO Report

        if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword))
            && node.Identifier.ValueText.Equals(_generateNames.BuildStagesMethod)
            && node.Body is { } body)
        {
            if (body.Statements.Count == 1
                && body.Statements[0] is ExpressionStatementSyntax statementSyntax
                && statementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                var stagesExpressionAnalyzer = new BuildStagesExpressionAnalyzer(this);
                invocationExpressionSyntax.Accept(stagesExpressionAnalyzer);
            }
        }
    }

    #endregion Public 方法

    #region Private 方法

    private void AddStage(string name)
    {
        _stages.Add(new(name, $"{_generateNames.WorkflowDescriptor.Name}{name}Stage"));
    }

    #endregion Private 方法

    #region Private 类

    private class BuildStagesExpressionAnalyzer : CSharpSyntaxWalker
    {
        #region Private 字段

        private readonly WorkflowStagesAnalyzer _workflowStagesAnalyzer;

        #endregion Private 字段

        #region Public 属性

        public bool IsBeginInvoked { get; private set; } = false;

        public bool IsCompletionInvoked { get; private set; } = false;

        #endregion Public 属性

        #region Public 构造函数

        public BuildStagesExpressionAnalyzer(WorkflowStagesAnalyzer workflowStagesAnalyzer)
        {
            _workflowStagesAnalyzer = workflowStagesAnalyzer;
        }

        #endregion Public 构造函数

        #region Public 方法

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            switch (node.Name.Identifier.ValueText)
            {
                case FluentWorkflowGeneratorConstants.CodeNames.StageBuilderBegin:
                    IsBeginInvoked = true;
                    break;

                case FluentWorkflowGeneratorConstants.CodeNames.StageBuilderThen:
                    {
                        if (IsCompletionInvoked
                            && node.Parent is InvocationExpressionSyntax invocationExpressionSyntax
                            && invocationExpressionSyntax.ArgumentList.Arguments.Count == 1
                            && invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalExpressionSyntax)
                        {
                            _workflowStagesAnalyzer.AddStage(literalExpressionSyntax.Token.ValueText);
                        }
                    }
                    break;

                case FluentWorkflowGeneratorConstants.CodeNames.StageBuilderCompletion:
                    IsCompletionInvoked = true;
                    break;
            }

            base.VisitMemberAccessExpression(node);
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
