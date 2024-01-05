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
|   Marshall_AWS | .NET 6.0 | .NET 6.0 | 6,282.1 ns | 122.15 ns | 119.97 ns | 2.0370 |    8534 B |
|    Marshall_SG | .NET 6.0 | .NET 6.0 |   794.6 ns |  10.45 ns |   9.78 ns | 0.9947 |    4160 B |
| Unmarshall_AWS | .NET 6.0 | .NET 6.0 | 6,860.9 ns |  30.40 ns |  25.39 ns | 1.5717 |    6593 B |
|  Unmarshall_SG | .NET 6.0 | .NET 6.0 |   300.4 ns |   3.53 ns |   3.13 ns | 0.0896 |     376 B |
|   Marshall_AWS | .NET 8.0 | .NET 8.0 | 4,548.0 ns |  86.94 ns |  89.28 ns | 1.9989 |    8390 B |
|    Marshall_SG | .NET 8.0 | .NET 8.0 |   921.7 ns |  13.56 ns |  12.02 ns | 0.9651 |    4040 B |
| Unmarshall_AWS | .NET 8.0 | .NET 8.0 | 4,464.9 ns |  70.99 ns |  66.40 ns | 1.5488 |    6505 B |
|  Unmarshall_SG | .NET 8.0 | .NET 8.0 |   253.2 ns |   5.13 ns |   5.04 ns | 0.0801 |     336 B |
