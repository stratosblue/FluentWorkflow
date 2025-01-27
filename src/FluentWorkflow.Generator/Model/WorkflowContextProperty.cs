using Microsoft.CodeAnalysis;

namespace FluentWorkflow.Generator.Model;

public record struct WorkflowContextProperty(string Name, ITypeSymbol Type, string? Comment);
