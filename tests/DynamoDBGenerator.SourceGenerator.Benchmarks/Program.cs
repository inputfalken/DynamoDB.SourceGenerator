// See https://aka.ms/new-console-template for more information


using BenchmarkDotNet.Running;
using DynamoDBGenerator.SourceGenerator.Benchmarks;

BenchmarkRunner.Run<MarshallBenchmark>();