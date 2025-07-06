Command (executed from rooth of repository):

`dotnet run --project tests/DynamoDBGenerator.SourceGenerator.Benchmarks --configuration 'Release' -- --exporters 'MarkDown' --filter '*Marshalling.Comparison*' --memory --job 'Default'`

Result:

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4484)
Intel Core Ultra 9 185H, 1 CPU, 22 logical and 16 physical cores
.NET SDK 9.0.107
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```

| Method                       | Mean        | Error     | StdDev      | Median      | Gen0   | Gen1   | Allocated |
|----------------------------- |------------:|----------:|------------:|------------:|-------:|-------:|----------:|
| Unmarshall_Person_DTO        |  1,276.3 ns |   8.46 ns |     7.50 ns |  1,276.6 ns | 0.0553 |      - |     696 B |
| Amazon_Unmarshall_Person_DTO | 10,349.0 ns | 558.41 ns | 1,637.72 ns | 11,053.8 ns | 0.9155 |      - |   11610 B |
| Marshall_Person_DTO          |    698.8 ns |   9.83 ns |     9.65 ns |    699.2 ns | 0.3052 | 0.0038 |    3840 B |
| Amazon_Marshall_Person_DTO   |  5,772.4 ns |  92.02 ns |    86.08 ns |  5,759.5 ns | 0.9460 |      - |   12076 B |
