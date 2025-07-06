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
| Method                       | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Unmarshall_Person_DTO        |   821.6 ns |  8.43 ns |  7.47 ns | 0.0553 |      - |     696 B |
| Amazon_Unmarshall_Person_DTO | 6,639.1 ns | 79.64 ns | 74.50 ns | 0.9155 |      - |   11609 B |
| Marshall_Person_DTO          |   689.6 ns |  4.24 ns |  3.76 ns | 0.3052 | 0.0038 |    3840 B |
| Amazon_Marshall_Person_DTO   | 5,611.1 ns | 20.94 ns | 17.49 ns | 0.9460 |      - |   12076 B |
