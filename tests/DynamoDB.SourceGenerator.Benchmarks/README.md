```
BenchmarkDotNet v0.13.6, Windows 10 (10.0.19045.3693/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
```

|         Method |      Job |  Runtime |       Mean |    Error |   StdDev |   Gen0 | Allocated |
--------------- |--------- |--------- |------------|----------|----------|--------|-----------|
| Marshall_AWS | .NET 6.0 | .NET 6.0 | 6,138.8 ns |  9.65 ns |  9.02 ns | 2.0370 |    8534 B |
| Marshall_SG | .NET 6.0 | .NET 6.0 |   732.6 ns |  3.47 ns |  3.25 ns | 0.9928 |    4152 B |
| Unmarshall_SG | .NET 6.0 | .NET 6.0 |   276.7 ns |  1.30 ns |  1.21 ns | 0.0877 |     368 B |
| Unmarshall_AWS | .NET 6.0 | .NET 6.0 | 6,853.0 ns | 11.67 ns | 10.92 ns | 1.5717 |    6593 B |
| Marshall_AWS | .NET 8.0 | .NET 8.0 | 4,285.5 ns | 11.43 ns | 10.69 ns | 1.9989 |    8390 B |
| Marshall_SG | .NET 8.0 | .NET 8.0 |   835.3 ns |  3.21 ns |  2.68 ns | 0.9632 |    4032 B |
| Unmarshall_SG | .NET 8.0 | .NET 8.0 |   239.0 ns |  0.44 ns |  0.41 ns | 0.0782 |     328 B |
| Unmarshall_AWS | .NET 8.0 | .NET 8.0 | 4,054.6 ns | 16.44 ns | 15.37 ns | 1.5488 |    6505 B |
