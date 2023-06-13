using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator.Model;

record struct WorkflowDescriptor(ClassDeclarationSyntax DeclarationSyntax,
                                 INamedTypeSymbol TypeSymbol,
                                 string Name,
                                 string NameSpace);
