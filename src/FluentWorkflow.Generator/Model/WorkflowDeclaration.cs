using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator.Model;

public record struct WorkflowDeclaration(ClassDeclarationSyntax DeclarationSyntax, string NameSpace, string DeclarationName, string WorkflowName, ImmutableArray<string> Stages, ImmutableArray<WorkflowContextProperty> ContextProperties);
