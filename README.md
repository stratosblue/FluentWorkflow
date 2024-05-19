# FluentWorkflow

A message driven distributed asynchronous workflow framework. 消息驱动的分布式异步工作流程处理框架。

## 1. Intro

基于消息驱动的分布式异步工作流程处理框架，使用 `SourceGenerator` 简化开发中的重复工作。

### 使用场景
 - 典型的消息驱动处理流程

![sample_message_driven_work_flow](./assets/sample_message_driven_work_flow.svg)

在典型的消息驱动处理流程中，阶段的`开始消息`与`结束消息`、各个消息的触发都需要手动定义，这些多数属于重复工作，`FluentWorkflow`是为了减少这些重复劳动而诞生的

-------

## 2. Features

- 基础代码自动生成，开发时只需要关注业务；
- 跨实例、跨服务工作流程驱动；
- 灵活的子工作流程等待/工作流程嵌套；
- 灵活的拓展性（`partial`/继承）；
- `Diagnostic`支持；
- 目标框架 `net7.0`+；
- *针对单个消息类型的Qos；

### NOTE:
- 更新包时应当`尽可能`的`全链路更新`，避免导致的未知问题；
- `WorkflowContext` 核心为 `字符串字典` 其属性在赋值时进行序列化存放，对象后续的修改不会反应到上下文中；

## 3. 开始使用

### 3.1 引用 `FluentWorkflow.Core` 包
```xml
<ItemGroup>
  <PackageReference Include="FluentWorkflow.Core" Version="1.0.0-*" />
</ItemGroup>
```

-------

### 3.2 定义工作流程

#### 声明一个工作流程类

```C#
public partial class SampleWorkflow : IWorkflow
{}
```

- 声明类型为 `partial`;
- 继承接口 `IWorkflow`;

-------

#### 此时代码生成器会自动为其继承基类，手动实现基类并定义工作流程

```C#
public partial class SampleWorkflow : IWorkflow
{
    public SampleWorkflow(SampleWorkflowContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }

    protected override void BuildStages(ISampleWorkflowStageBuilder stageBuilder)
    {
        stageBuilder.Begin()
                    .Then("SampleStage1")
                    .Then("SampleStage2")
                    .Then("SampleStage3")
                    .Completion();
    }
}
```

到此一个工作流程就声明完成了，该工作流程名为`SampleWorkflow`，包含三个阶段 `SampleStage1` -> `SampleStage2` -> `SampleStage3`

- 工作流程在`BuildStages`方法中使用参数`stageBuilder`定义，必须链式调用，由`Begin()`开始`Completion()`结束，使用`Then("StageName")`声明每个阶段，声明顺序即为阶段顺序，阶段名称必须满足[C#标识符命名规则和约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/identifier-names)；
- 代码生成器已为工作流程生成了必要的工作代码: 
    - 工作流程上下文 `SampleWorkflowContext`    (模板：`{WorkflowName}Context`)
    - 工作流程消息 - 每个阶段的开始完成消息等    (模板：`{WorkflowName}{StageName}(Stage|StageCompleted)Message`)
    - 阶段处理器基类 `SampleWorkflowSampleStage1StageHandlerBase`、`SampleWorkflowSampleStage2StageHandlerBase`、`SampleWorkflowSampleStage3StageHandlerBase`    (模板：`{WorkflowName}{StageName}StageHandlerBase`)
    - *其它相关支撑类型

-------

### 3.3 实现阶段处理器

#### 继承对应的`阶段处理器基类`，并实现各个阶段处理逻辑
```C#
// SampleStage2 与 SampleStage3 同理
public class SampleWorkflowSampleStage1StageHandler : SampleWorkflowSampleStage1StageHandlerBase
{
    public SampleWorkflowSampleStage1StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Task ProcessAsync(ProcessContext processContext, SampleWorkflowSampleStage1StageMessage stageMessage, CancellationToken cancellationToken)
    {
        //TODO 阶段业务逻辑
        return Task.CompletedTask;
    }
}
```

-------

### 3.4 配置服务

#### 配置`控制`服务

```C#
services.AddFluentWorkflow()
        .AddSampleWorkflowScheduler()   //添加工作流程调度器
        .AddSampleWorkflowResultObserver()          //添加结果观察器
        .UseInMemoryWorkflowMessageDispatcher();    //配置使用的消息分发器，这里使用基于内存的分发器来示范
```

-------

#### 配置`阶段处理`服务

```C#
services.AddFluentWorkflow()
        .AddSampleWorkflowSampleStage1StageHandler<SampleWorkflowSampleStage1StageHandler>()    //添加对应阶段的处理器, SampleStage2 与 SampleStage3 同理
        .UseInMemoryWorkflowMessageDispatcher();    //配置使用的消息分发器，这里使用基于内存的分发器来示范
```

-------

`FluentWorkflow`正常工作的必要条件: 
 - 流程中的所有`服务`使用`同一套`消息分发器;
 - 有且仅配置了一个（单个服务，可多实例）工作流程调度器 - `WorkflowScheduler`;
 - 所有阶段的阶段处理器 - `StageHandler`，各个阶段的阶段处理器有且仅有一个（单个服务，可多实例）;
 - *需要等待`子工作流程`时必须配置子工作流程结果观察器 - `ResultObserver`;
 - *需要单次等待多个`子工作流程`时，必须使用支持等待多个`子工作流程`的 `IWorkflowAwaitProcessor`; (默认实现了基于`redis`的多流程等待处理器，配置时使用`UseRedisWorkflowAwaitProcessor`方法以启用)

### 3.5 启动工作流程

```C#
//从DI容器中获取工作流程构建器
var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();
//创建工作流程上下文
var context = new SampleWorkflowContext();
//构建工作流程
var workflow = workflowBuilder.Build(context);
//启动工作流程，框架会自动触发各个阶段处理器后完成
await workflow.StartAsync(default);
```

## 4. 注意事项

- 启动工作流程的服务可以不是配置工作流程调度器 - `WorkflowScheduler`的服务，但需要接入`消息分发器`并在配置时使用 `Add****Workflow()` 添加对应的工作流程构造器；
- 源代码生成器生成的绝大部分类型都是`partial`的，可以声明`partial`类进行拓展，不可使用`partial`类拓展的功能基本上都可以继承后重写，在配置服务时替换默认实现即可；
- 定义的 `Workflow` 类会添加生命周期各个阶段的触发事件方法，可以继承后重写其逻辑以在各个阶段执行相关的逻辑（注意每次触发可能不在同一个服务实例中）；
- `WorkflowContext` 核心为`字符串字典`，对其修改理论上只对后续可见并在整个执行周期可用，可以将执行参数、结果、中间值等存放其中；
- 消息的分发、重试等逻辑由具体使用的消息分发器`IWorkflowMessageDispatcher`控制（默认提供了基于`CAP`、`Abp`以及基础的`FluentWorkflow.RabbitMQ`可选）；
- 默认情况下 `StageHandler` 出现异常则认为工作流程失败，不会将异常抛给上层 `IWorkflowMessageDispatcher`（消息分发的重试不会触发），可以重写 `StageHandler` 的 `OnException` 方法来将异常向上抛出；
- 更改既有工作流程时，如果`修改`/`删除`了既有的阶段定义，会导致还在处理过程中工作流程无法正常运行（但添加不会影响）；

-------

## 5. 其它

### 5.1 生成拓展功能代码
部分功能为源码接入的方式，默认不生成，在项目中指定需要的功能后自动生成
```C#
<PropertyGroup>
  <FluentWorkflowGeneratorAdditional>AbpFoundation,CAPFoundation,AbpMessageDispatcher,CAPMessageDispatcher,RedisAwaitProcessor</FluentWorkflowGeneratorAdditional>
</PropertyGroup>
```

|名称|功能|
|--|--|
|AbpFoundation|Abp的基础功能支持|
|CAPFoundation|CAP的基础功能支持|
|AbpMessageDispatcher|Abp的消息分发器|
|CAPMessageDispatcher|CAP的消息分发器|
|RedisAwaitProcessor|基于`StackExchange.Redis`的子流程等待处理器|

* 生成的可能冲突的类型会放到命名空间 `FluentWorkflow.GenericExtension.{工作流程命名空间}` 下，如配置拓展方法等；

### 5.2 使用默认分发器 `FluentWorkflow.RabbitMQ`

#### 引用 `FluentWorkflow.RabbitMQ` 包
```xml
<ItemGroup>
    <PackageReference Include="FluentWorkflow.RabbitMQ" Version="1.0.0-*" />
</ItemGroup>
```
#### 配置
```C#
services.AddFluentWorkflow()
        .UseRabbitMQMessageDispatcher(options =>
        {
            //配置RabbitMQ
        });
```

-------

#### *控制指定消息的消费速率
配置单个消息的消费速率，其它消息不受限
```C#
services.Configure<RabbitMQOptions>(options =>
{
    //配置阶段Stage1的消息 - SampleWorkflowSampleStage1StageMessage 的消费速率，即当前服务实例同时只会有一个阶段Stage1在处理
    options.Option<SampleWorkflowSampleStage1StageMessage>(static handleOptions =>
    {
        handleOptions.Qos = 1;
    });
});
```

#### *消息确认超时

RabbitMQ消息的消费ack超时时间默认为30分钟，进行长时间处理时可能会出现意外情况，可参照 [acknowledgement-timeout](https://www.rabbitmq.com/docs/consumers#acknowledgement-timeout) 进行调整

-------

### 5.3 子工作流程等待

在阶段处理器中实现子工作流程等待逻辑

```C#
internal class SampleWorkflowSampleStage1StageHandler : SampleWorkflowSampleStage1StageHandlerBase
{
    public SampleWorkflowSampleStage1StageHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected async override Task ProcessAsync(ProcessContext processContext, SampleWorkflowSampleStage1StageMessage stageMessage, CancellationToken cancellationToken)
    {
        //构建子工作流程
        var workflowBuilder = ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflow>>();
        var workflow = workflowBuilder.Build(new SampleWorkflowContext());
        //Other workflow setting

        //将未启动的子工作流程传递给当前阶段处理上下文，并命名为 - 'taskName'
        processContext.AwaitChildWorkflow("taskName", workflow);

        // Other logic

        //当前阶段将等待子工作流程处理完成后再完成
    }

    protected override async Task OnAwaitFinishedAsync(SampleWorkflowContext context, IReadOnlyDictionary<string, IWorkflowContext?> childWorkflowContexts, CancellationToken cancellationToken)
    {
        //从等待的子工作流程上下文字典中取出 - 'taskName'
        var workflowContext = (SampleWorkflowContext)childWorkflowContexts["taskName"];

        //处理子工作流程结果，如将 workflowContext 内的结果赋值给 context，以便在当前工作流程的后续阶段中使用等

        await base.OnAwaitFinishedAsync(context, childWorkflowContexts, cancellationToken);

        //当前阶段将完成
    }
}
```

-------

### 5.4 启用`Diagnostic`支持
```C#
services.AddFluentWorkFlow().EnableDiagnostic();
```

-------

## 6 流程的中止、挂起与恢复

### 6.1 中止流程
在 `WorkFlow` 的 `On*StageAsync` 和 `On*StageCompletedAsync` 中不执行参数委托 `fireMessage`，则后续流程不再执行

### 6.2 流程挂起
在 `WorkFlow` 的 `On*StageAsync` 和 `On*StageCompletedAsync` 中不执行参数委托 `fireMessage`，中止流程，在此基础上调用 `SerializeContext` 方法将上下文序列化后存放
```C#
// 存放 contextData 以用于流程恢复
var contextData = SerializeContext(message.Context);
```

### 6.3 流程恢复
调用具体 `WorkFlow` 的静态方法 `ResumeAsync` 使用挂起的流程数据进行恢复执行
```C#
// contextData 为序列化的上下文数据
await XXXXWorkflow.ResumeAsync(contextData, serviceProvider, cancellationToken)
```

#### 注意:
恢复流程将会再次调用序列化上下文时的方法，需要注意，小心再次被挂起

-------

更多信息参见源码内的测试代码
