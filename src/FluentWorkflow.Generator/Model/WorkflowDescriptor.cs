using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator.Model;

internal record struct WorkflowDeclarationDescriptor(ClassDeclarationSyntax DeclarationSyntax, string Name, string NameSpace);

internal record struct WorkflowDescriptor(ClassDeclarationSyntax DeclarationSyntax, string Name, string NameSpace)
    : IEquatable<WorkflowDescriptor>
{
    /// <summary>
    /// 短名称
    /// </summary>
    public string ShortName { get; } = CreateShortName(Name);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        //认为名称和命名空间相同则相同？
        return Name.GetHashCode() & NameSpace.GetHashCode();
    }

    /// <inheritdoc/>
    public readonly bool Equals(WorkflowDescriptor other)
    {
        return string.Equals(Name, other.Name, StringComparison.Ordinal)
               && string.Equals(NameSpace, other.NameSpace, StringComparison.Ordinal);
    }

    private static string CreateShortName(string name)
    {
        return name.EndsWith("Workflow", StringComparison.OrdinalIgnoreCase)
               ? name.Substring(0, name.Length - "Workflow".Length)
               : name;
    }
}

internal sealed class WorkflowDescriptorEqualityComparer : IEqualityComparer<WorkflowDescriptor>
{
    #region Public 属性

    public static WorkflowDescriptorEqualityComparer Shared { get; } = new();

    #endregion Public 属性

    #region Public 方法

    public bool Equals(WorkflowDescriptor x, WorkflowDescriptor y) => x.Equals(y);

    public int GetHashCode(WorkflowDescriptor obj) => obj.GetHashCode();

    #endregion Public 方法
}
