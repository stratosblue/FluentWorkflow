using System.Text.Json;
using FluentWorkflow.Interface;
using TemplateNamespace;

namespace FluentWorkflow;

[TestClass]
public class WorkflowContextTest
{
    #region Public 方法

    [TestMethod]
    public void Should_Set_Value_Success()
    {
        var context = new TemplateWorkflowContext(Guid.NewGuid().ToString());
        context.SetValue("v1", DateTime.Now);
        //context.MyProperty = 16545.564;
        var json = JsonSerializer.Serialize(context);
        //var jsonn = Newtonsoft.Json.JsonConvert.SerializeObject(context);
        //TODO 其它序列化组件的支持

        var restoreContext = JsonSerializer.Deserialize<TemplateWorkflowContext>(json);
        Assert.IsNotNull(restoreContext);
        //var restoreContextn = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleWorkflowContext>(jsonn);

        var icontext = context as IWorkflowContext;
        Assert.IsNotNull(icontext);

        var icontextn = restoreContext as IWorkflowContext;
        Assert.IsNotNull(icontextn);

        var rawkvs = icontext.GetSnapshot();
        var rawkvsn = icontextn.GetSnapshot();

        Assert.AreEqual(rawkvs.Count(), rawkvsn.Count());

        foreach (var kv in rawkvs)
        {
            var rawValue = restoreContext.Get(kv.Key);
            Assert.AreEqual(kv.Value, rawValue);
        }
    }

    [TestMethod]
    public void Should_Throw_For_Set_ProtectedKey()
    {
        var context = new TemplateWorkflowContext(Guid.NewGuid().ToString());
        Assert.ThrowsException<InvalidOperationException>(() => context.Set(FluentWorkflowConstants.ContextKeys.Id, "value"));
        Assert.ThrowsException<InvalidOperationException>(() => context.Set(FluentWorkflowConstants.ContextKeys.Stage, "value"));
        Assert.ThrowsException<InvalidOperationException>(() => context.Set(FluentWorkflowConstants.ContextKeys.WorkflowName, "value"));
    }

    #endregion Public 方法
}
