using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

[DynamoDBMarshaller(EntityType = typeof(Container<bool>), AccessName = "BooleanMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<char>), AccessName = "CharMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<int>), AccessName = "Int32Marshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<long>), AccessName = "Int64Marshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<string>), AccessName = "StringMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<uint>), AccessName = "UInt32Marshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<ulong>), AccessName = "UInt64Marshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<Guid>), AccessName = "GuidMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>), AccessName = "EnumMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<double>), AccessName = "DoubleMarshaller")]
public partial class PrimitiveBenchmarks
{
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<bool>, Container<bool>, ContainerboolNames, ContainerboolValues> _bool = BooleanMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<char>, Container<char>, ContainercharNames, ContainercharValues> _char = CharMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<int>, Container<int>, ContainerintNames, ContainerintValues> _int = Int32Marshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<long>, Container<long>, ContainerlongNames, ContainerlongValues> _long = Int64Marshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<string>, Container<string>, ContainerstringNames, ContainerstringValues> _string = StringMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<uint>, Container<uint>, ContaineruintNames, ContaineruintValues> _uint = UInt32Marshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<ulong>, Container<ulong>, ContainerulongNames, ContainerulongValues> _ulong = UInt64Marshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<Guid>, Container<Guid>, ContainerGuidNames, ContainerGuidValues> _guid = GuidMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<DayOfWeek>, Container<DayOfWeek>, ContainerDayOfWeekNames, ContainerDayOfWeekValues> _enum = EnumMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<double>, Container<double>, ContainerdoubleNames, ContainerdoubleValues> _double = DoubleMarshaller.ToBenchmarkHelper();
    
    [Benchmark]
    public Container<bool> Unmarshall_Bool() => _bool.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Bool() => _bool.Marshall();

    [Benchmark]
    public Container<char> Unmarshall_Char() => _char.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Char() => _char.Marshall();

    [Benchmark]
    public Container<int> Unmarshall_Int32() => _int.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Int() => _int.Marshall();
    
    [Benchmark]
    public Container<long> Unmarshall_Int64() => _long.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Int64() => _long.Marshall();
    
    [Benchmark]
    public Container<string> Unmarshall_String() => _string.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_String() => _string.Marshall();
    
    [Benchmark]
    public Container<uint> Unmarshall_UInt32() => _uint.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_UInt32() => _uint.Marshall();
    
    [Benchmark]
    public Container<ulong> Unmarshall_UInt64() => _ulong.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Uint64() => _ulong.Marshall();
    
    [Benchmark]
    public Container<Guid> Unmarshall_Guid() => _guid.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Guid() => _guid.Marshall();
    
    [Benchmark]
    public Container<DayOfWeek> Unmarshall_Enum() => _enum.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Enum() => _enum.Marshall();
    
    [Benchmark]
    public Container<double> Unmarshall_Double() => _double.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Double() => _double.Marshall();
}