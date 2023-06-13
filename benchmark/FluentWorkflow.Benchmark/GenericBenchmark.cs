using BenchmarkDotNet.Attributes;

namespace FluentWorkflow;

//TODO 待完成

[SimpleJob]
[MemoryDiagnoser]
public class GenericBenchmark
{
    [Params(2, 8)]
    public int Param1 { get; set; }

    [Benchmark]
    public void BenchmarkMethod()
    {
    }

    [IterationSetup]
    public void IterationSetup()
    {
    }
}
