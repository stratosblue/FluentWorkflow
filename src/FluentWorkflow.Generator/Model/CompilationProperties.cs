namespace FluentWorkflow.Generator.Model;

internal record struct CompilationProperties(string RootNameSpace, HashSet<GeneratorAdditional> GeneratorAdditionals);
