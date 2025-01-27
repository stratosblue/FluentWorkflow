namespace Microsoft.CodeAnalysis;

internal static class TypeSymbolExtensions
{
    #region Private 字段

    private static readonly SymbolDisplayFormat s_fullNameFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    #endregion Private 字段

    #region Public 方法

    public static bool EqualsDefault(this ISymbol typeSymbol, ISymbol? compareSymbol)
    {
        return typeSymbol.Equals(compareSymbol, SymbolEqualityComparer.Default);
    }

    public static string ToFullCodeString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string ToFullCodeString(this INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static string ToFullCodeString(this IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.Type.ToFullCodeString();
    }

    public static string ToFullNameString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(s_fullNameFormat).Replace('.', '_').Replace('<', '_').Replace('>', '_');
    }

    public static string ToFullNameString(this INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.ToDisplayString(s_fullNameFormat).Replace('.', '_');
    }

    public static string ToFullNameString(this IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.Type.ToFullNameString();
    }

    /// <summary>
    /// 形如 IEnumerable<>
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static string ToGenericNoTypeArgumentFullCodeString(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return typeSymbol.ToFullCodeString();
        }
        return $"{typeSymbol.ContainingNamespace.ToFullCodeString()}.{typeSymbol.Name}<{GetCommas(namedTypeSymbol.TypeArguments.Length)}>";

        string GetCommas(int argumentCount)
        {
            return argumentCount switch
            {
                0 => throw new ArgumentOutOfRangeException(),
                1 => "",
                2 => ",",
                3 => ",,",
                4 => ",,,",
                5 => ",,,,",
                _ => new string(Enumerable.Repeat(',', argumentCount - 1).ToArray()),
            };
        }
    }

    public static string ToRemarkCodeString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToFullCodeString().Replace('<', '{').Replace('>', '}');
    }

    #endregion Public 方法
}
