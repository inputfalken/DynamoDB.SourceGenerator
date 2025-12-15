```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
Intel Core Ultra 9 185H 2.50GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3


```
| Method                       | Mean       | Error     | StdDev    | Median     | Gen0   | Gen1   | Allocated |
|----------------------------- |-----------:|----------:|----------:|-----------:|-------:|-------:|----------:|
| Unmarshall_Person_DTO        |   645.2 ns |   9.16 ns |   8.12 ns |   645.7 ns | 0.0553 |      - |     696 B |
| Amazon_Unmarshall_Person_DTO | 5,291.8 ns | 101.03 ns | 112.30 ns | 5,312.2 ns | 0.7935 | 0.0076 |   10041 B |
| Marshall_Person_DTO          |   812.4 ns |  42.03 ns | 122.60 ns |   859.8 ns | 0.3052 | 0.0038 |    3840 B |
| Amazon_Marshall_Person_DTO   | 8,633.3 ns | 146.56 ns | 137.09 ns | 8,617.5 ns | 0.9613 | 0.0153 |   12084 B |
