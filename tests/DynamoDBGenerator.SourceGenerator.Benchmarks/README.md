```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4541/23H2/2023Update/SunValley3)
Intel Core Ultra 9 185H, 1 CPU, 22 logical and 16 physical cores
.NET SDK 8.0.307
  [Host]   : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0  

```
| Method         | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Marshall_AWS   | 2,478.4 ns | 18.54 ns | 28.31 ns | 0.6676 | 0.0076 |    8386 B |
| Marshall_SG    |   428.6 ns |  5.97 ns | 10.76 ns | 0.3076 | 0.0038 |    3864 B |
| Unmarshall_AWS | 2,271.2 ns | 17.33 ns | 16.21 ns | 0.5150 | 0.0038 |    6504 B |
| Unmarshall_SG  |   126.4 ns |  0.65 ns |  0.58 ns | 0.0126 |      - |     160 B |
