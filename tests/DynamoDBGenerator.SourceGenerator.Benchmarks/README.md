```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2


```
|         Method |      Job |  Runtime |       Mean |     Error |    StdDev |   Gen0 | Allocated |
|--------------- |--------- |--------- |-----------:|----------:|----------:|-------:|----------:|
|   Marshall_AWS | .NET 6.0 | .NET 6.0 | 6,148.5 ns | 120.28 ns | 164.64 ns | 2.0370 |    8534 B |
|    Marshall_SG | .NET 6.0 | .NET 6.0 |   786.1 ns |  14.87 ns |  40.95 ns | 0.9422 |    3944 B |
| Unmarshall_AWS | .NET 6.0 | .NET 6.0 | 7,116.8 ns | 141.55 ns | 228.57 ns | 1.5717 |    6593 B |
|  Unmarshall_SG | .NET 6.0 | .NET 6.0 |   253.9 ns |   4.52 ns |   4.23 ns | 0.0381 |     160 B |
|   Marshall_AWS | .NET 8.0 | .NET 8.0 | 4,517.5 ns |  87.92 ns | 128.87 ns | 1.9989 |    8390 B |
|    Marshall_SG | .NET 8.0 | .NET 8.0 |   816.9 ns |  16.19 ns |  13.52 ns | 0.9232 |    3864 B |
| Unmarshall_AWS | .NET 8.0 | .NET 8.0 | 4,187.4 ns |  83.13 ns | 143.40 ns | 1.5488 |    6505 B |
|  Unmarshall_SG | .NET 8.0 | .NET 8.0 |   174.5 ns |   1.81 ns |   1.51 ns | 0.0381 |     160 B |
