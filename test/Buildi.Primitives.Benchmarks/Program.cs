using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
    DefaultConfig.Instance.WithSummaryStyle(BenchmarkDotNet.Reports.SummaryStyle.Default));
