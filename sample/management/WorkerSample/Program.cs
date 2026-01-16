using System.Net;
using FluentWorkflow;
using FluentWorkflow.Management.Worker;
using FluentWorkflow.WorkerSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json")
                     .AddUserSecrets<Program>();

var sampleAddress = builder.Configuration.GetValue<string>("SampleAddress")!;
var sampleCookie = builder.Configuration.GetValue<string>("SampleCookie")!;

var services = builder.Services;

services.AddFluentWorkflow()
        .UseInMemoryWorkflowMessageDispatcher()
        .AddSampleWorkflowWorkflow(configuration =>
        {
            configuration.AddScheduler()
                         .AddResultObserver()
                         .AddStageProcessHandler<StageProcessHandler>();
        })
        .UseWorkingController();

services.AddFluentWorkflowManagementWorker(IPEndPoint.Parse(sampleAddress), sampleCookie);

var host = builder.Build();
await host.StartAsync();

_ = Enumerable.Range(0, 2).Select(async _ =>
{
    return Task.Run(async () =>
    {
        while (true)
        {
            await using var asyncServiceScope = host.Services.CreateAsyncScope();
            var builder = asyncServiceScope.ServiceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflowWorkflow>>();
            var context = new SampleWorkflowWorkflowContext()
            {
                Delay = TimeSpan.FromSeconds(Random.Shared.Next(5, 100)),
            };
            await builder.Build(context).StartAsync(default);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    });
}).ToList();

await host.WaitForShutdownAsync();
