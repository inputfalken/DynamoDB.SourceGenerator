```

BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3803/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0  

```
|         Method |       Mean |    Error |   StdDev |   Gen0 | Allocated |
|--------------- |-----------:|---------:|---------:|-------:|----------:|
|   Marshall_AWS | 4,520.9 ns | 86.77 ns | 96.45 ns | 1.9989 |    8390 B |
|    Marshall_SG |   869.9 ns | 10.49 ns |  9.30 ns | 0.9651 |    4040 B |
| Unmarshall_AWS | 4,165.9 ns | 34.13 ns | 28.50 ns | 1.5488 |    6505 B |
|  Unmarshall_SG |   234.7 ns |  1.68 ns |  1.40 ns | 0.0801 |     336 B |
