
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4484)
Intel Core Ultra 9 185H, 1 CPU, 22 logical and 16 physical cores
.NET SDK 9.0.107
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


 Method                    | Mean       | Error     | StdDev    | Gen0   | Gen1   | Allocated |
-------------------------- |-----------:|----------:|----------:|-------:|-------:|----------:|
 Unmarshall_Person_DTO     |   828.4 ns |  11.89 ns |  11.12 ns | 0.0553 |      - |     696 B |
 Marshall_Person_DTO       |   722.2 ns |  11.13 ns |   9.87 ns | 0.3052 | 0.0038 |    3840 B |
 AWS_Unmarshall_Person_DTO | 6,712.2 ns | 130.69 ns | 199.57 ns | 0.9155 |      - |   11610 B |
 AWS_Marshall_Person_DTO   | 5,984.9 ns | 118.61 ns | 181.13 ns | 0.9460 |      - |   12076 B |
