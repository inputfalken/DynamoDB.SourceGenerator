using Amazon.DynamoDBv2.Model;
using AutoFixture;
using BenchmarkDotNet.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks;

public abstract class SG_BenchMarker<T>
{
    private readonly Func<T, Dictionary<string, AttributeValue>> _marshaller;
    private readonly Func<Dictionary<string, AttributeValue>, T> _unMarshaller;
    protected readonly T SingleElement;
    protected readonly Dictionary<string, AttributeValue> AttributeValues;

    protected SG_BenchMarker(
        Func<T, Dictionary<string, AttributeValue>> marshaller,
        Func<Dictionary<string, AttributeValue>, T> unMarshaller
    )
    {
        _marshaller = marshaller;
        _unMarshaller = unMarshaller;
        SingleElement = SetupFixture().Create<T>();
        AttributeValues = marshaller(SingleElement);
    }

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Source_Generated() => _marshaller(SingleElement);

    [Benchmark]
    public T Unmarshall_Source_Generated() => _unMarshaller(AttributeValues);

    private static Fixture SetupFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<DateOnly>(o => o.FromFactory((DateTime dt) => DateOnly.FromDateTime(dt)));
        fixture.Customize<TimeOnly>(o => o.FromFactory((DateTime dt) => TimeOnly.FromDateTime(dt)));
        // Allow recursive types
        fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}