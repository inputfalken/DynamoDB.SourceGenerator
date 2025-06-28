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
| Marshall_AWS_Reflection     | 5,351.9 ns | 74.57 ns | 69.75 ns | 0.8545 |      - |   10875 B |
| Marshall_Source_Generated   |   488.9 ns |  8.21 ns |  7.68 ns | 0.2375 | 0.0019 |    2984 B |
| Unmarshall_AWS_Reflection   | 5,447.3 ns | 68.30 ns | 63.89 ns | 0.8545 |      - |   10922 B |
| Unmarshall_Source_Generated |   923.6 ns | 10.92 ns | 10.21 ns | 0.0610 |      - |     768 B |
