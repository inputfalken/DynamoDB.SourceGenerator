using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

[DynamoDBMarshaller(EntityType = typeof(Container<DateTime>), AccessName = "DateTimeMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<DateTimeOffset>), AccessName = "DateTimeOffsetMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<DateOnly>), AccessName = "DateOnlyMarshaller")]
[DynamoDBMarshaller(EntityType = typeof(Container<TimeOnly>), AccessName = "TimeOnlyMarshaller")]
public partial class TemporalBenchmarks
{
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<DateTime>, Container<DateTime>, ContainerDateTimeNames, ContainerDateTimeValues> _dateTime = DateTimeMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<DateTimeOffset>, Container<DateTimeOffset>, ContainerDateTimeOffsetNames, ContainerDateTimeOffsetValues> _dateTimeOffset = DateTimeOffsetMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<DateOnly>, Container<DateOnly>, ContainerDateOnlyNames, ContainerDateOnlyValues> _dateOnly = DateOnlyMarshaller.ToBenchmarkHelper();
    private readonly DynamoDbMarshallerBenchmarkHelper<Container<TimeOnly>, Container<TimeOnly>, ContainerTimeOnlyNames, ContainerTimeOnlyValues> _timeOnly = TimeOnlyMarshaller.ToBenchmarkHelper();

    [Benchmark]
    public Container<TimeOnly> Unmarshall_TimeOnly() => _timeOnly.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_TimeOnly() => _timeOnly.Marshall();

    [Benchmark]
    public Container<DateOnly> Unmarshall_DateOnly() => _dateOnly.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_DateOnly() => _dateOnly.Marshall();

    [Benchmark]
    public Container<DateTimeOffset> Unmarshall_DateTimeOffset() => _dateTimeOffset.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_DateTimeOffset() => _dateTimeOffset.Marshall();

    [Benchmark]
    public Container<DateTime> Unmarshall_DateTime() => _dateTime.Unmarshall();

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_DateTime() => _dateTime.Marshall();
}