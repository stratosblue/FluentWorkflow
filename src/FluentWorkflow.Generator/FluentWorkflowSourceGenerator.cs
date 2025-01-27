using System.Reflection;
using System.Text;
using FluentWorkflow.Generator.Model;
using FluentWorkflow.Generator.Providers;
using FluentWorkflow.Generator.Providers.Workflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FluentWorkflow.Generator;

[Generator(LanguageNames.CSharp)]
public class FluentWorkflowSourceGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        InitializePreCodes(context);

        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterWorkflowDeclarationSyntaxNode, TransformWorkflowDeclarationSyntaxNode)
                                                         .Collect()
                                                         .SelectMany((items, _) => items.Distinct());

        var compilationPropertiesProvider = context.AnalyzerConfigOptionsProvider.Select((configOptions, token) =>
        {
            var generatorAdditionals = new HashSet<GeneratorAdditional>();

            if (configOptions.GlobalOptions.TryGetValue("build_property.FluentWorkflowGeneratorAdditional", out var generatorAdditionalValues)
                && !string.IsNullOrWhiteSpace(generatorAdditionalValues))
            {
                foreach (var generatorAdditionalValue in generatorAdditionalValues.Split([',', ';', ' ', '\t']))
                {
                    if (Enum.TryParse<GeneratorAdditional>(generatorAdditionalValue, out var value))
                    {
                        generatorAdditionals.Add(value);
                    }
                }
            }

            configOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNameSpace);

            return new CompilationProperties(RootNameSpace: rootNameSpace ?? string.Empty,
                                             GeneratorAdditionals: generatorAdditionals.ToList());
        });

        context.RegisterSourceOutput(declarationsProvider,
                                     (context, input) =>
                                     {
                                         var workflowDeclaration = input;

                                         context.AddSource(new DeclarationBaseSourceProvider(workflowDeclaration));
                                         context.AddSource(new DeclarationSourceProvider(workflowDeclaration));
                                     });

        //context.RegisterSourceOutput(declarationsProvider.Combine(compilationPropertiesProvider),
        //                             (context, input) =>
        //                             {
        //                                 var workflowDescriptor = input.Left;
        //                                 var properties = input.Right;

        //                                 var generateContext = new GenerateContext(workflowDescriptor);

        //                                 context.AddSource(new BaseSourceProvider(generateContext));
        //                                 context.AddSource(new BuilderSourceProvider(generateContext));
        //                                 context.AddSource(new ContextSourceProvider(generateContext));
        //                                 context.AddSource(new DIExtensionsSourceProvider(generateContext));
        //                                 context.AddSource(new DIExtensionsConfigurationSourceProvider(generateContext));
        //                                 context.AddSource(new DIExtensionsStageHandlerSourceProvider(generateContext));
        //                                 context.AddSource(new MessagesSourceProvider(generateContext));
        //                                 context.AddSource(new ResultObserverSourceProvider(generateContext));
        //                                 context.AddSource(new SchedulerSourceProvider(generateContext));
        //                                 context.AddSource(new StageBuilderSourceProvider(generateContext));
        //                                 context.AddSource(new StageContinuatorsSourceProvider(generateContext));
        //                                 context.AddSource(new StageHandlerSourceProvider(generateContext));
        //                                 context.AddSource(new StagesSourceProvider(generateContext));
        //                                 context.AddSource(new StartRequestHandlerSourceProvider(generateContext));
        //                                 context.AddSource(new StateMachineSourceProvider(generateContext));
        //                                 context.AddSource(new StateMachineDriverSourceProvider(generateContext));

        //                                 if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.AbpFoundation))
        //                                 {
        //                                     context.AddSource(new AbpMessagesSourceProvider(generateContext));
        //                                     context.AddSource(new AbpResultObserverSourceProvider(generateContext));
        //                                     context.AddSource(new AbpStageHandlersSourceProvider(generateContext));
        //                                     context.AddSource(new AbpStartRequestHandlerSourceProvider(generateContext));
        //                                     context.AddSource(new AbpStateMachineDriverSourceProvider(generateContext));
        //                                 }

        //                                 if (properties.GeneratorAdditionals.Contains(GeneratorAdditional.CAPFoundation))
        //                                 {
        //                                     context.AddSource(new CapResultObserverSourceProvider(generateContext));
        //                                     context.AddSource(new CapStageHandlersSourceProvider(generateContext));
        //                                     context.AddSource(new CapStartRequestHandlerSourceProvider(generateContext));
        //                                     context.AddSource(new CapStateMachineDriverSourceProvider(generateContext));
        //                                 }
        //                             });

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

    private static bool FilterWorkflowDeclarationSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Count == 0
            && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
            && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword)
            && IsBaseOnIWorkflowDeclarationDirectly(classDeclarationSyntax))
        {
            return true;
        }
        return false;

        static bool IsBaseOnIWorkflowDeclarationDirectly(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var types = classDeclarationSyntax.BaseList?.Types;
            if (types is null
                || types.Value.Count == 0)
            {
                return false;
            }
            return types.OfType<SimpleBaseTypeSyntax>()
                        .Any(m => m.Type is IdentifierNameSyntax identifierNameSyntax && string.Equals(identifierNameSyntax.Identifier.ValueText, "IWorkflowDeclaration", StringComparison.Ordinal));
        }
    }

    private static WorkflowDeclaration TransformWorkflowDeclarationSyntaxNode(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
        var workflowDeclarationAnalyzer = new WorkflowDeclarationAnalyzer(classDeclarationSyntax, syntaxContext.SemanticModel);
        workflowDeclarationAnalyzer.Visit(classDeclarationSyntax);

        return workflowDeclarationAnalyzer.WorkflowDeclaration;
    }

    #endregion filter

    private static void InitializePreCodes(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            var assembly = typeof(FluentWorkflowSourceGenerator).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            AddPreCodes(context, assembly, resourceNames.Single(m => m.EndsWith("IWorkflowDeclaration.cs")));
            AddPreCodes(context, assembly, resourceNames.Single(m => m.EndsWith("WorkflowDeclarations.cs")));
        });

        static void AddPreCodes(IncrementalGeneratorPostInitializationContext context, Assembly assembly, string resourceName)
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(resourceStream);
            var code = reader.ReadToEnd();

            context.AddSource(Path.GetFileName(resourceName), SourceText.From(code, Encoding.UTF8));
        }
    }

    #endregion Private 方法
}
