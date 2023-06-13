using FluentWorkflow.Generator.Model;

namespace FluentWorkflow.Generator.Providers;

internal abstract class SourceProvider
{
    #region Public 方法

    public abstract IEnumerable<GeneratedSource?>? Generate();

    #endregion Public 方法
}
