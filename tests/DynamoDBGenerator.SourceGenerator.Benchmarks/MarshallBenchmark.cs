using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class MarshallBenchmark
{
    private readonly DynamoDBContext _context;
    private readonly ToDocumentConfig _dynamoDbOperationConfig;
    private readonly PersonEntity _singleElement;
    private readonly Dictionary<string, AttributeValue> _attributeValues;


    public MarshallBenchmark()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _context = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient(RegionEndpoint.EUWest1))
            .Build();
        _dynamoDbOperationConfig = new()
        {
            Conversion = DynamoDBEntryConversion.V2
        };

        _singleElement = fixture.Create<PersonEntity>();
        _attributeValues = _context.ToDocument(_singleElement).ToAttributeMap();
    }


    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_AWS_Reflection()
    {
        return _context.ToDocument(_singleElement, _dynamoDbOperationConfig)
            .ToAttributeMap(_dynamoDbOperationConfig.Conversion);
    }

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_Source_Generated()
    {
        return PersonEntity.PersonEntityMarshaller.Marshall(_singleElement);
    }

    [Benchmark]
    public PersonEntity Unmarshall_AWS_Reflection()
    {
        return _context.FromDocument<PersonEntity>(Document.FromAttributeMap(_attributeValues));
    }

    [Benchmark]
    public PersonEntity Unmarshall_Source_Generated()
    {
        return PersonEntity.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }
}