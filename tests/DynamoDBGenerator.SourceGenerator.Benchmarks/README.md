Command (executed from rooth of repository):

`dotnet run --project tests/DynamoDBGenerator.SourceGenerator.Benchmarks --configuration 'Release' -- --exporters 'MarkDown' --filter '*Marshalling.Comparison*' --memory --job 'Default'`

Result:

```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7171)
Intel Core Ultra 9 185H 2.50GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3


```
| Method                       | Mean       | Error    | StdDev    | Gen0   | Gen1   | Allocated |
|----------------------------- |-----------:|---------:|----------:|-------:|-------:|----------:|
| Unmarshall_Person_DTO        |   655.8 ns |  7.40 ns |   6.92 ns | 0.0553 |      - |     696 B |
| Amazon_Unmarshall_Person_DTO | 4,929.2 ns | 97.26 ns | 129.83 ns | 0.7935 | 0.0076 |   10041 B |
| Marshall_Person_DTO          |   548.0 ns |  5.46 ns |   5.11 ns | 0.3052 | 0.0038 |    3840 B |
| Amazon_Marshall_Person_DTO   | 4,396.5 ns | 23.98 ns |  22.43 ns | 0.9460 |      - |   12084 B |
