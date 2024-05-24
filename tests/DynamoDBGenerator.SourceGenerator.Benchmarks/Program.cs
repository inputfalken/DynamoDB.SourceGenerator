// See https://aka.ms/new-console-template for more information

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DynamoDBGenerator.SourceGenerator.Benchmarks.Models;
using DynamoDBGenerator.SourceGenerator.Benchmarks;


BenchmarkRunner.Run<Marshalling>();

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class Marshalling
{
    private readonly DynamoDBContext _context;
    private readonly DynamoDBOperationConfig _dynamoDbOperationConfig;
    private readonly PersonEntity _singleElement;
    private readonly Dictionary<string, AttributeValue> _attributeValues;

    public Marshalling()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _context = new(new AmazonDynamoDBClient(RegionEndpoint.EUWest1));
        _dynamoDbOperationConfig = new()
        {
            Conversion = DynamoDBEntryConversion.V2
        };

        _singleElement = fixture.Create<PersonEntity>();
        _attributeValues = _context.ToDocument(_singleElement).ToAttributeMap();
    }


    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_AWS()
    {
        return _context.ToDocument(_singleElement, _dynamoDbOperationConfig).ToAttributeMap(_dynamoDbOperationConfig.Conversion);
    }

    [Benchmark]
    public Dictionary<string, AttributeValue> Marshall_SG()
    {
        return PersonEntity.PersonEntityMarshaller.Marshall(_singleElement);
    }

    [Benchmark]
    public PersonEntity Unmarshall_AWS()
    {
        return _context.FromDocument<PersonEntity>(Document.FromAttributeMap(_attributeValues));
    }

    [Benchmark]
    public PersonEntity Unmarshall_SG()
    {
        return PersonEntity.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }

}
