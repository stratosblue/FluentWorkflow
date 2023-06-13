using FluentWorkflow.Generator.Model;
using FluentWorkflow.Generator.Providers;
using FluentWorkflow.Generator.Providers.Workflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentWorkflow.Generator;

[Generator(LanguageNames.CSharp)]
public class FluentWorkflowSourceGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterContextSyntaxNode, TransformContextSyntaxNode);

        var compilationPropertiesProvider = context.AnalyzerConfigOptionsProvider.Select((configOptions, token) =>
        {
            var generatorAdditionals = new HashSet<GeneratorAdditional>();

            if (configOptions.GlobalOptions.TryGetValue("build_property.FluentWorkflowGeneratorAdditional", out var generatorAdditionalValues)
                && !string.IsNullOrWhiteSpace(generatorAdditionalValues))
            {
                foreach (var generatorAdditionalValue in generatorAdditionalValues.Split(new[] { ',', ';', ' ', '\t' }))
                {
                    if (Enum.TryParse<GeneratorAdditional>(generatorAdditionalValue, out var value))
                    {
                        generatorAdditionals.Add(value);
                    }
                }
            }

            configOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNameSpace);

            return new CompilationProperties()
            {
                RootNameSpace = rootNameSpace ?? string.Empty,
                GeneratorAdditionals = generatorAdditionals,
            };
        });

        context.RegisterSourceOutput(declarationsProvider.Combine(compilationPropertiesProvider),
                                     (context, input) =>
                                     {
                                         var workflowDescriptor = input.Left;
                                         var properties = input.Right;

                                         var generateContext = new GenerateContext(workflowDescriptor);

                                         context.AddSource(new BaseSourceProvider(generateContext));
                                         context.AddSource(new BuilderSourceProvider(generateContext));
                                         context.AddSource(new ContextSourceProvider(generateContext));
                                         context.AddSource(new DIExtensionsSourceProvider(generateContext));
                                         context.AddSource(new DIExtensionsSchedulerSourceProvider(generateContext));
                                         context.AddSource(new DIExtensionsStageHandlerSourceProvider(generateContext));
                                         context.AddSource(new MessagesSourceProvider(generateContext));
                                         context.AddSource(new ResultObserverSourceProvider(generateContext));
                                         context.AddSource(new SchedulerSourceProvider(generateContext));
                                         context.AddSource(new StageBuilderSourceProvider(generateContext));
                                         context.AddSource(new StageContinuatorsSourceProvider(generateContext));
                                         context.AddSource(new StageHandlerSourceProvider(generateContext));
                                         context.AddSource(new StagesSourceProvider(generateContext));
                                         context.AddSource(new StartRequestHandlerSourceProvider(generateContext));
                                         context.AddSource(new StateMachineSourceProvider(generateContext));
                                         context.AddSource(new StateMachineDriverSourceProvider(generateContext));

                                         if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.AbpFoundation))
                                         {
                                             context.AddSource(new AbpMessagesSourceProvider(generateContext));
                                             context.AddSource(new AbpResultObserverSourceProvider(generateContext));
                                             context.AddSource(new AbpStageHandlersSourceProvider(generateContext));
                                             context.AddSource(new AbpStartRequestHandlerSourceProvider(generateContext));
                                             context.AddSource(new AbpStateMachineDriverSourceProvider(generateContext));
                                         }

                                         if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.CAPFoundation))
                                         {
                                             context.AddSource(new CapResultObserverSourceProvider(generateContext));
                                             context.AddSource(new CapStageHandlersSourceProvider(generateContext));
                                             context.AddSource(new CapStartRequestHandlerSourceProvider(generateContext));
                                             context.AddSource(new CapStateMachineDriverSourceProvider(generateContext));
                                         }
                                     });

        context.RegisterSourceOutput(compilationPropertiesProvider,
                                     (context, properties) =>
                                     {
                                         var nameSpacePostFix = string.IsNullOrWhiteSpace(properties.RootNameSpace)
                                                                ? string.Empty
                                                                : $".{properties.RootNameSpace.Replace(".", string.Empty)}";
                                         if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.AbpMessageDispatcher))
                                         {
                                             context.AddSource(new AbpDistributedEventBusWorkflowMessageDispatcherSourceProvider(nameSpacePostFix));
                                         }

                                         if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.CAPMessageDispatcher))
                                         {
                                             context.AddSource(new CapPublisherWorkflowMessageDispatcherSourceProvider(nameSpacePostFix));
                                         }

                                         if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.RedisAwaitProcessor))
                                         {
                                             context.AddSource(new RedisWorkflowAwaitProcessorSourceProvider(nameSpacePostFix));
                                         }
                                     });
    }

    #endregion Public 方法

    #region Private 方法

    #region filter

    private static bool FilterContextSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Count == 0
            && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword)
            && IsBaseOnIWorkflowDirectly(classDeclarationSyntax))
        {
            return true;
        }
        return false;

        static bool IsBaseOnIWorkflowDirectly(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var types = classDeclarationSyntax.BaseList?.Types;
            if (types is null)
            {
                return false;
            }
            return types.OfType<SimpleBaseTypeSyntax>()
                        .Any(m => m.Type is IdentifierNameSyntax identifierNameSyntax && string.Equals(identifierNameSyntax.Identifier.ValueText, "IWorkflow", StringComparison.Ordinal));
        }
    }

    private static WorkflowDescriptor TransformContextSyntaxNode(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
        var typeSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken)!;

        var workflowName = classDeclarationSyntax.Identifier.ValueText;
        var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();

        return new(classDeclarationSyntax, typeSymbol, workflowName, nameSpace);
    }

    #endregion filter

    #endregion Private 方法
}
