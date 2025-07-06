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

| Method                       |       Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
|------------------------------|-----------:|----------:|----------:|-------:|-------:|----------:|
| Unmarshall_Person_DTO        |   681.6 ns |   6.95 ns |   6.50 ns | 0.0553 |      - |     696 B |
| Amazon_Unmarshall_Person_DTO | 6,131.7 ns |  42.92 ns |  38.05 ns | 0.9155 |      - |   11610 B |
| Marshall_Person_DTO          |   694.6 ns |  13.15 ns |  12.30 ns | 0.3052 | 0.0038 |    3840 B |
| Amazon_Marshall_Person_DTO   | 5,824.9 ns | 116.27 ns | 264.80 ns | 0.9460 |      - |   12076 B |
