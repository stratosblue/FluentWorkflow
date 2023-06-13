using System.Text;
using FluentWorkflow.Generator.Model;
using FluentWorkflow.Generator.Providers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace FluentWorkflow.Generator;

internal static class SourceProductionContextExtensions
{
    #region Public 方法

    public static SourceProductionContext AddSource(this SourceProductionContext context, SourceProvider? sourceProvider)
    {
        if (sourceProvider is not null
            && sourceProvider.Generate() is { } generatedSources)
        {
            foreach (var source in generatedSources)
            {
                context.AddSource(source);
            }
        }
        return context;
    }

    public static SourceProductionContext AddSource(this SourceProductionContext context, GeneratedSource? generatedSource)
    {
        if (generatedSource.HasValue)
        {
            var hintName = generatedSource.Value.HitName;
            var sourceCode = generatedSource.Value.SourceCode;
            context.AddSource(hintName, Format(sourceCode));
        }
        return context;
    }

    public static SourceText Format(string text, CancellationToken cancellationToken = default)
    {
        var codeSyntaxTree = CSharpSyntaxTree.ParseText(text: text, options: new CSharpParseOptions(LanguageVersion.LatestMajor), encoding: Encoding.UTF8, cancellationToken: cancellationToken);

        var sourceText = codeSyntaxTree.GetRoot(cancellationToken).NormalizeWhitespace().SyntaxTree.GetText(cancellationToken);

        return sourceText;
    }

    #endregion Public 方法
}
