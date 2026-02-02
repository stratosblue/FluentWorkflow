using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator.Model;

internal class WorkflowDeclarationAnalyzer : CSharpSyntaxWalker
{
    #region Private 字段

    private readonly ClassDeclarationSyntax _classDeclarationSyntax;

    private readonly string _declarationName;

    private readonly string _nameSpace;

    private readonly List<WorkflowContextProperty> _properties = [];

    private readonly SemanticModel _semanticModel;

    private readonly List<string> _stages = [];

    private string? _workflowName;

    #endregion Private 字段

    #region Public 属性

    public WorkflowDeclaration WorkflowDeclaration
    {
        get
        {
            return new(DeclarationSyntax: _classDeclarationSyntax,
                       NameSpace: _nameSpace,
                       DeclarationName: _declarationName,
                       WorkflowName: _workflowName ?? "Undefined",
                       Stages: ImmutableArray.Create((_stages as IEnumerable<string>).Reverse().ToArray()),
                       ContextProperties: ImmutableArray.Create((_properties as IEnumerable<WorkflowContextProperty>).Reverse().ToArray()));
        }
    }

    #endregion Public 属性

    #region Public 构造函数

    public WorkflowDeclarationAnalyzer(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel)
    {
        _classDeclarationSyntax = classDeclarationSyntax;
        _semanticModel = semanticModel;

        _declarationName = classDeclarationSyntax.Identifier.ValueText;
        var namespaceNameSyntax = classDeclarationSyntax.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>()?.Name
                                  ?? classDeclarationSyntax.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name;
        _nameSpace = namespaceNameSyntax!.ToFullString();
    }

    #endregion Public 构造函数

    #region 工具方法

    public static bool TryGetWorkflowDeclaration(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel, out WorkflowDeclaration workflowDeclaration)
    {
        try
        {
            var workflowDeclarationAnalyzer = new WorkflowDeclarationAnalyzer(classDeclarationSyntax, semanticModel);
            workflowDeclarationAnalyzer.Visit(classDeclarationSyntax);

            if (workflowDeclarationAnalyzer._workflowName is not null)
            {
                workflowDeclaration = workflowDeclarationAnalyzer.WorkflowDeclaration;
                return true;
            }
        }
        catch
        {
            //HACK 解决方案内项目引用时，有语法树，但无法通过语法树获取类型信息，静默所有异常
        }
        workflowDeclaration = default;
        return false;
    }

    #endregion 工具方法

    #region Public 方法

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        //TODO Report

        if (node.Modifiers.Any(static m => m.IsKind(SyntaxKind.OverrideKeyword))
            && node.Identifier.ValueText.Equals("DeclareWorkflow")
            && node.Body is { } declareWorkflowBody)
        {
            if (declareWorkflowBody.Statements.Count == 1
                && declareWorkflowBody.Statements[0] is ExpressionStatementSyntax statementSyntax
                && statementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                var workflowDeclarationExpressionAnalyzer = new WorkflowDeclarationExpressionAnalyzer(this);
                invocationExpressionSyntax.Accept(workflowDeclarationExpressionAnalyzer);
            }
        }
        else if (node.Modifiers.Any(static m => m.IsKind(SyntaxKind.OverrideKeyword))
                 && node.Identifier.ValueText.Equals("DeclareContext")
                 && node.Body is { } declareContextBody)
        {
            if (declareContextBody.Statements.Count == 1
                && declareContextBody.Statements[0] is ExpressionStatementSyntax statementSyntax
                && statementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                var contextDeclarationExpressionAnalyzer = new ContextDeclarationExpressionAnalyzer(this);
                invocationExpressionSyntax.Accept(contextDeclarationExpressionAnalyzer);
            }
        }
    }

    #endregion Public 方法

    #region Private 方法

    private void AddStage(string name)
    {
        _stages.Add(name);
    }

    #endregion Private 方法

    #region Private 类

    private class ContextDeclarationExpressionAnalyzer(WorkflowDeclarationAnalyzer workflowDeclarationAnalyzer) : CSharpSyntaxWalker
    {
        #region Public 方法

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            switch (node.Name.Identifier.ValueText)
            {
                case FluentWorkflowGeneratorConstants.CodeNames.PropertyDefineMethod:
                    {
                        if (node.Parent is InvocationExpressionSyntax invocationExpressionSyntax
                            && invocationExpressionSyntax.ArgumentList.Arguments is { } arguments
                            && arguments.Count > 0
                            && node.Name is GenericNameSyntax genericNameSyntax
                            && genericNameSyntax.TypeArgumentList.Arguments is { } typeArguments
                            && typeArguments.Count == 1)
                        {
                            var name = (arguments[0].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                            var typeInfo = workflowDeclarationAnalyzer._semanticModel.GetTypeInfo(typeArguments[0]);
                            var comment = arguments.Count > 1 ? (arguments[1].Expression as LiteralExpressionSyntax)?.Token.ValueText : null;
                            if (name is not null
                                && typeInfo.Type is not null)
                            {
                                workflowDeclarationAnalyzer._properties.Add(new(name, typeInfo.Type, comment));
                            }
                        }
                    }
                    break;
            }

            base.VisitMemberAccessExpression(node);
        }

        #endregion Public 方法
    }

    private class WorkflowDeclarationExpressionAnalyzer(WorkflowDeclarationAnalyzer workflowDeclarationAnalyzer) : CSharpSyntaxWalker
    {
        #region Public 属性

        public bool IsBeginInvoked { get; private set; } = false;

        public bool IsCompletionInvoked { get; private set; } = false;

        #endregion Public 属性

        #region Public 方法

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            switch (node.Name.Identifier.ValueText)
            {
                case FluentWorkflowGeneratorConstants.CodeNames.WorkflowName:
                    {
                        if (node.Parent is InvocationExpressionSyntax invocationExpressionSyntax
                             && invocationExpressionSyntax.ArgumentList.Arguments.Count == 1
                             && invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalExpressionSyntax)
                        {
                            workflowDeclarationAnalyzer._workflowName = literalExpressionSyntax.Token.ValueText;
                        }
                    }
                    break;

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
                            workflowDeclarationAnalyzer.AddStage(literalExpressionSyntax.Token.ValueText);
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
