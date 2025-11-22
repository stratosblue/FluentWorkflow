using System.Text.Json;
using FluentWorkflow.SimpleSample;
using Microsoft.Extensions.DependencyInjection;

namespace FluentWorkflow;

[TestClass]
public class WorkflowContextTest
{
    #region Public 方法

    [TestMethod]
    public void Should_Build_Workflow_With_Same_Instance()
    {
        var context = new SampleWorkflowContext(Guid.NewGuid().ToString());

        var services = new ServiceCollection();
        services.AddFluentWorkflow()
                .AddSampleWorkflow<SampleWorkflowImpl>();

        using var serviceProvider = services.BuildServiceProvider();
        var builder = serviceProvider.GetRequiredService<IWorkflowBuilder<SampleWorkflowImpl>>();

        var workflow = builder.Build(context);
        Assert.IsTrue(ReferenceEquals(context, workflow.Context));

        workflow = builder.Build((context as IWorkflowContext).GetSnapshot().ToDictionary(m => m.Key, m => m.Value));
        Assert.IsFalse(ReferenceEquals(context, workflow.Context));
    }

    [TestMethod]
    public void Should_Set_Value_Success()
    {
        var context = new SampleWorkflowContext(Guid.NewGuid().ToString());
        context.SetValue("v1", DateTime.Now);
        //context.MyProperty = 16545.564;
        var json = JsonSerializer.Serialize(context);
        //var jsonn = Newtonsoft.Json.JsonConvert.SerializeObject(context);
        //TODO 其它序列化组件的支持

        var restoreContext = JsonSerializer.Deserialize<SampleWorkflowContext>(json);
        Assert.IsNotNull(restoreContext);
        //var restoreContextn = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleWorkflowContext>(jsonn);

        var icontext = context as IWorkflowContext;
        Assert.IsNotNull(icontext);

        var icontextn = restoreContext as IWorkflowContext;
        Assert.IsNotNull(icontextn);

        var rawkvs = icontext.GetSnapshot();
        var rawkvsn = icontextn.GetSnapshot();

        Assert.HasCount(rawkvs.Count, rawkvsn);

        foreach (var kv in rawkvs)
        {
            var rawValue = rawkvsn[kv.Key];
            Assert.AreEqual(kv.Value, rawValue);
        }
    }

    [TestMethod]
    public void Should_Throw_For_Set_ProtectedKey()
    {
        var context = new SampleWorkflowContext(Guid.NewGuid().ToString());
        Assert.ThrowsExactly<InvalidOperationException>(() => context.SetValue(FluentWorkflowConstants.ContextKeys.Metadata, "value"));
        Assert.ThrowsExactly<InvalidOperationException>(() => context.SetValue(FluentWorkflowConstants.ContextKeys.State, "value"));
        Assert.ThrowsExactly<InvalidOperationException>(() => context.SetValue(FluentWorkflowConstants.ContextKeys.ParentWorkflow, "value"));
    }

    #endregion Public 方法
}
