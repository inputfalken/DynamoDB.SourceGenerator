```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2


```
|         Method |      Job |  Runtime |       Mean |     Error |    StdDev |     Median |   Gen0 |   Gen1 | Allocated |
|--------------- |--------- |--------- |-----------:|----------:|----------:|-----------:|-------:|-------:|----------:|
|   Marshall_AWS | .NET 6.0 | .NET 6.0 | 6,099.9 ns | 105.35 ns | 266.24 ns | 5,991.2 ns | 2.0370 |      - |    8534 B |
|    Marshall_SG | .NET 6.0 | .NET 6.0 |   781.3 ns |  12.06 ns |  10.69 ns |   783.9 ns | 0.9947 |      - |    4160 B |
| Unmarshall_AWS | .NET 6.0 | .NET 6.0 | 6,647.3 ns |  24.94 ns |  23.33 ns | 6,651.9 ns | 1.5717 |      - |    6593 B |
|  Unmarshall_SG | .NET 6.0 | .NET 6.0 |   298.4 ns |   1.31 ns |   1.16 ns |   298.6 ns | 0.0896 | 0.0005 |     376 B |
|   Marshall_AWS | .NET 8.0 | .NET 8.0 | 4,263.3 ns |  24.79 ns |  23.19 ns | 4,268.2 ns | 1.9989 |      - |    8390 B |
|    Marshall_SG | .NET 8.0 | .NET 8.0 |   831.8 ns |   3.32 ns |   3.11 ns |   831.6 ns | 0.9651 |      - |    4040 B |
| Unmarshall_AWS | .NET 8.0 | .NET 8.0 | 4,131.3 ns |  20.88 ns |  16.30 ns | 4,129.5 ns | 1.5488 |      - |    6505 B |
|  Unmarshall_SG | .NET 8.0 | .NET 8.0 |   227.4 ns |   1.63 ns |   1.52 ns |   226.6 ns | 0.0801 |      - |     336 B |
