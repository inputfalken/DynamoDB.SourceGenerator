using BenchmarkDotNet.Running;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);