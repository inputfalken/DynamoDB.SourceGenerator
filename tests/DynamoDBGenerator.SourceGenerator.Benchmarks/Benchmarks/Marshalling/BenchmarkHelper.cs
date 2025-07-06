using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.Model;
using AutoFixture;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling;

public class DynamoDbMarshallerBenchmarkHelper<T, T2, T3, T4> 
    where T4 : IAttributeExpressionValueTracker<T2>
    where T3 : IAttributeExpressionNameTracker
{
    private readonly IDynamoDBMarshaller<T, T2, T3, T4> _marshaller;
    private readonly T _element;
    private readonly Dictionary<string, AttributeValue> _attributeValues;

    public DynamoDbMarshallerBenchmarkHelper(IDynamoDBMarshaller<T, T2, T3, T4> marshaller)
    {
        _marshaller = marshaller;
        _element = SetupFixture().Create<T>();
        _attributeValues = marshaller.Marshall(_element);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unmarshall() => _marshaller.Unmarshall(_attributeValues);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<string, AttributeValue> Marshall() => _marshaller.Marshall(_element);

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

public static class Extensions
{
    public static DynamoDbMarshallerBenchmarkHelper<T, T2, T3, T4> ToBenchmarkHelper<T, T2, T3, T4>(
        this IDynamoDBMarshaller<T, T2, T3, T4> marshaller) where T3 : IAttributeExpressionNameTracker
        where T4 : IAttributeExpressionValueTracker<T2>
    {
        return new DynamoDbMarshallerBenchmarkHelper<T, T2, T3, T4>(marshaller);
    }
}