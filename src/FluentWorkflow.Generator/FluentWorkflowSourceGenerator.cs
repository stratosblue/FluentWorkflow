using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using FluentWorkflow.Generator;
using FluentWorkflow.Generator.Model;
using FluentWorkflow.Generator.Providers;
using FluentWorkflow.Generator.Providers.Workflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FluentWorkflow;

[Generator(LanguageNames.CSharp)]
public class FluentWorkflowSourceGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        InitializePreCodes(context);

        //声明提供器
        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterWorkflowDeclarationSyntaxNode, TransformWorkflowDeclarationSyntaxNode)
                                                         .Collect()
                                                         .SelectMany((items, _) => items.Distinct());

        //生成定义提供器
        var generationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterGenerationDeclarationSyntaxNode, TransformGenerationDeclarationSyntaxNode)
                                                 .Collect()
                                                 .SelectMany((items, _) => items.Distinct());

        //编译属性提供器
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

        //生成声明信息
        context.RegisterSourceOutput(declarationsProvider,
                                     (context, input) =>
                                     {
                                         var workflowDeclaration = input;

                                         context.AddSource(new DeclarationBaseSourceProvider(workflowDeclaration));
                                         context.AddSource(new DeclarationSourceProvider(workflowDeclaration));
                                     });

        //生成定义信息
        context.RegisterSourceOutput(generationsProvider.Combine(compilationPropertiesProvider),
                                     (context, input) =>
                                     {
                                         var workflowGenerationDescriptors = input.Left;
                                         var properties = input.Right;

                                         foreach (var descriptor in workflowGenerationDescriptors)
                                         {
                                             var (workflowDeclaration, generationMode) = descriptor;

                                             if (generationMode == WorkflowSourceGenerationMode.Default)
                                             {
                                                 generationMode = WorkflowSourceGenerationMode.All;
                                             }

                                             var generateContext = new GenerateContext(workflowDeclaration);

                                             if (generationMode.HasFlag(WorkflowSourceGenerationMode.Workflow))
                                             {
                                                 context.AddSource(new BaseSourceProvider(generateContext));
                                                 context.AddSource(new BuilderSourceProvider(generateContext));
                                                 context.AddSource(new ContextSourceProvider(generateContext));
                                                 context.AddSource(new DIExtensionsSourceProvider(generateContext));
                                                 context.AddSource(new DIExtensionsConfigurationSourceProvider(generateContext));
                                             }

                                             if (generationMode.HasFlag(WorkflowSourceGenerationMode.Messages))
                                             {
                                                 context.AddSource(new MessagesSourceProvider(generateContext));
                                             }

                                             if (generationMode.HasFlag(WorkflowSourceGenerationMode.MessageHandlers))
                                             {
                                                 context.AddSource(new ResultObserverSourceProvider(generateContext));
                                                 context.AddSource(new StageContinuatorsSourceProvider(generateContext));
                                                 context.AddSource(new StageHandlerSourceProvider(generateContext));
                                                 context.AddSource(new StagesSourceProvider(generateContext));

                                                 context.AddSource(new DIExtensionsStageHandlerSourceProvider(generateContext));
                                             }

                                             if (generationMode.HasFlag(WorkflowSourceGenerationMode.Scheduler))
                                             {
                                                 context.AddSource(new SchedulerSourceProvider(generateContext));
                                                 context.AddSource(new StartRequestHandlerSourceProvider(generateContext));
                                                 context.AddSource(new StateMachineSourceProvider(generateContext));
                                                 context.AddSource(new StateMachineDriverSourceProvider(generateContext));
                                             }

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

    #region generation

    private bool FilterGenerationDeclarationSyntaxNode(SyntaxNode node, CancellationToken token)
    {
        if (node is AttributeListSyntax attributeListSyntax
            && attributeListSyntax.Attributes is { Count: > 0 } attributes
            && string.Equals("assembly", attributeListSyntax.Target?.Identifier.ValueText, StringComparison.Ordinal)
            && attributes.Any(m => m.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1 && string.Equals("GenerateWorkflowCodes", genericNameSyntax.Identifier.ValueText)))
        {
            return true;
        }
        return false;
    }

    private HashSet<WorkflowGenerationDescriptor> TransformGenerationDeclarationSyntaxNode(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        var semanticModel = syntaxContext.SemanticModel;
        var attributeListSyntax = (AttributeListSyntax)syntaxContext.Node;
        var attributes = attributeListSyntax.Attributes;
        HashSet<WorkflowGenerationDescriptor> generationDescriptors = [];

        foreach (var attribute in attributes.Where(m => m.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1 && string.Equals("GenerateWorkflowCodes", genericNameSyntax.Identifier.ValueText)))
        {
            var genericNameSyntax = (GenericNameSyntax)attribute.Name;
            var typeSyntax = genericNameSyntax.TypeArgumentList.Arguments[0];

            var generationMode = WorkflowSourceGenerationMode.Default;

            //获取生成模式
            if (attribute.ArgumentList is { } argumentList
                && argumentList.Arguments.Count > 0)
            {
                var modeValue = argumentList.Arguments[0].Expression.ToFullString().Split('.').LastOrDefault();
                Enum.TryParse(modeValue, out generationMode);
            }

            var typeInfo = syntaxContext.SemanticModel.GetTypeInfo(typeSyntax);
            if (typeInfo.Type?.DeclaringSyntaxReferences.Length > 0)    //项目内定义，使用语法树解析
            {
                foreach (var syntaxReference in typeInfo.Type.DeclaringSyntaxReferences)
                {
                    if (syntaxReference.GetSyntax() is ClassDeclarationSyntax classDeclarationSyntax
                        && WorkflowDeclarationAnalyzer.TryGetWorkflowDeclaration(classDeclarationSyntax, semanticModel, out var workflowDeclaration))
                    {
                        generationDescriptors.Add(new(workflowDeclaration, generationMode));
                        break;
                    }
                }
            }
            else    //项目外定义，使用反射
            {
                //TODO 错误提示
                var declarationType = typeInfo.ConvertedType;
                var attributeDatas = declarationType?.GetAttributes();
                if (attributeDatas?.Length > 0
                    && attributeDatas.Value.FirstOrDefault(m => m.AttributeClass?.Name == "WorkflowDefineAttribute") is { } defineAttributeData
                    && defineAttributeData.ConstructorArguments is { } arguments
                    && arguments.Length >= 3)
                {
                    var version = (int)arguments[0].Value!;
                    if (version > (int)GeneratorVersion.Version2)
                    {
                        throw new InvalidOperationException($"Declaration version \"{version}\" not supported.");
                    }
                    var workflowName = (string)arguments[1].Value!;
                    var stages = arguments[2].Values.Select(m => (string)m.Value!).ToImmutableArray();

                    var contextPropertyAttributeDatas = attributeDatas.Value.Where(m => m.AttributeClass?.Name == "WorkflowContextTypedPropertyAttribute").ToList();
                    var contextProperties = contextPropertyAttributeDatas.Select(m =>
                                                                         {
                                                                             var propertyType = m.AttributeClass!.TypeArguments[0];
                                                                             var propertyName= (string)m.ConstructorArguments[0].Value!;
                                                                             var propertyComment = m.ConstructorArguments[1].Value as string;
                                                                             return new WorkflowContextProperty(propertyName, propertyType, propertyComment);
                                                                         })
                                                                         .ToImmutableArray();
                    var workflowDeclaration = new WorkflowDeclaration(null,
                                                                      declarationType!.ContainingNamespace.ToDisplayString(),
                                                                      declarationType.Name,
                                                                      workflowName,
                                                                      stages,
                                                                      contextProperties);

                    generationDescriptors.Add(new(workflowDeclaration, generationMode));
                }
            }
        }

        return generationDescriptors;
    }

    #endregion generation

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
