using System.Runtime.CompilerServices;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;

public class AWSComparisonBenchmarkHelper<T, T2, T3, T4>
    where T4 : IAttributeExpressionValueTracker<T2>
    where T3 : IAttributeExpressionNameTracker
{
    private readonly IDynamoDBMarshaller<T, T2, T3, T4> _marshaller;
    private readonly T _element;
    private readonly Dictionary<string, AttributeValue> _attributeValues;

    private readonly DynamoDBContext _context;

    private readonly ToDocumentConfig _dynamoDbOperationConfig;

    public AWSComparisonBenchmarkHelper(IDynamoDBMarshaller<T, T2, T3, T4> marshaller)
    {
        _marshaller = marshaller;
        _element = SetupFixture().Create<T>();
        _attributeValues = marshaller.Marshall(_element);
        _dynamoDbOperationConfig = new ToDocumentConfig { Conversion = DynamoDBEntryConversion.V2 };
        _context = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            .Build();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unmarshall() => _marshaller.Unmarshall(_attributeValues);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<string, AttributeValue> Marshall() => _marshaller.Marshall(_element);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<string, AttributeValue> Marshall_AWS() => _context
        .ToDocument(_element, _dynamoDbOperationConfig)
        .ToAttributeMap(_dynamoDbOperationConfig.Conversion);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unmarshall_AWS() =>
        _context.FromDocument<T>(Amazon.DynamoDBv2.DocumentModel.Document.FromAttributeMap(_attributeValues));

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