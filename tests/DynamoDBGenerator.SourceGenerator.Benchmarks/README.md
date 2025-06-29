```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4484)
Intel Core Ultra 9 185H, 1 CPU, 22 logical and 16 physical cores
.NET SDK 9.0.107
  [Host]   : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                      | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Marshall_AWS_Reflection     | 5,841.5 ns | 87.91 ns | 82.23 ns | 0.8850 |      - |   11309 B |
| Marshall_Source_Generated   |   508.5 ns |  9.08 ns | 19.16 ns | 0.2384 | 0.0019 |    3000 B |
| Unmarshall_AWS_Reflection   | 6,164.6 ns | 43.43 ns | 40.62 ns | 0.8850 |      - |   11274 B |
| Unmarshall_Source_Generated |   814.9 ns | 14.09 ns | 13.18 ns | 0.0305 |      - |     392 B |
