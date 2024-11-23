namespace FluentWorkflow.Generator.Model;

internal record struct CompilationProperties(string RootNameSpace, IReadOnlyList<GeneratorAdditional> GeneratorAdditionals)
    : IEquatable<CompilationProperties>
{
    private readonly IReadOnlyList<GeneratorAdditional> _orderedGeneratorAdditionals = GeneratorAdditionals.OrderBy(static m => (int)m).ToList();

    public override readonly int GetHashCode()
    {
        var hashCode = RootNameSpace.GetHashCode();
        foreach (var generatorAdditional in _orderedGeneratorAdditionals)
        {
            hashCode &= generatorAdditional.GetHashCode();
        }
        return hashCode;
    }

    public readonly bool Equals(CompilationProperties other)
    {
        return RootNameSpace.Equals(other.RootNameSpace)
               && Enumerable.SequenceEqual(_orderedGeneratorAdditionals, other._orderedGeneratorAdditionals);
    }
}
