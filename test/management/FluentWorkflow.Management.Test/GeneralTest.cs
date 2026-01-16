using System.Net;
using FluentWorkflow.Extensions;
using FluentWorkflow.Management.Worker;
using Hoarwell;
using Hoarwell.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentWorkflow.Management.Test;

[TestClass]
public sealed class GeneralTest
{
    #region Private 字段

    private IHost _clientHost = null!;

    private IHost _serverHost = null!;

    #endregion Private 字段

    #region Public 方法

    [TestMethod]
    public async Task Should_Connect_Success()
    {
        var managementClient = _clientHost.Services.CreateScope().ServiceProvider.GetRequiredKeyedService<ManagementClusterConnector>("default");
        await managementClient.ConnectAsync(default);
        await managementClient.PingAsync(default);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await _clientHost.DisposeAsync();
        await _serverHost.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        var cookie = Guid.NewGuid().ToString();
        EndPoint endPoint = IPEndPoint.Parse("127.0.0.1:0");

        _serverHost = Host.CreateDefaultBuilder()
                          .ConfigureServices(services =>
                          {
                              services.AddFluentWorkflowManagementManager(endPoint, cookie);
                          })
                          .Build();

        var runner = _serverHost.Services.GetRequiredKeyedService<IHoarwellApplicationRunner>("default");

        await runner.StartAsync();

        endPoint = runner.Features.RequiredFeature<ILocalEndPointsFeature>().EndPoints.First();

        _clientHost = Host.CreateDefaultBuilder()
                          .ConfigureServices(services =>
                          {
                              services.AddFluentWorkflow().UseWorkingController();
                              services.AddFluentWorkflowManagementWorker(endPoint, cookie);
                          })
                          .Build();
    }

    #endregion Public 方法
}
