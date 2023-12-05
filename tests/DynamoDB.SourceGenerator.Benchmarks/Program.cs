// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DynamoDB.SourceGenerator.Benchmarks;
using DynamoDB.SourceGenerator.Benchmarks.Models;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;


BenchmarkRunner.Run<Marshalling>();

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class Marshalling
{
    private readonly Repository _repository;
    private readonly DynamoDBContext _context;
    private readonly DynamoDBOperationConfig _dynamoDbOperationConfig;
    private readonly PersonEntity _singleElement;
    private readonly Dictionary<string, AttributeValue> _attributeValues;

    public Marshalling()
    {
        _repository = new();
        _context = new(new AmazonDynamoDBClient());
        _dynamoDbOperationConfig = new()
        {
            Conversion = DynamoDBEntryConversion.V2
        };

        _singleElement = new()
        {
            Id = "Abc",
            Firstname = "John",
            Lastname = "Doe",
            Address = new Address
            {
                Id = "SomeId",
                PostalCode = new PostalCode
                {
                    ZipCode = "123",
                    Town = "Abc"
                },
                Street = "Abc",
                Neighbours = new List<PersonEntity>()
            }
        };
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
        return _repository.PersonEntityMarshaller.Marshall(_singleElement);
    }
    
    [Benchmark]
    public PersonEntity Unmarshall_AWS()
    {
        return _context.FromDocument<PersonEntity>(Document.FromAttributeMap(_attributeValues));
    }

    [Benchmark]
    public PersonEntity Unmarshall_SG()
    {
        return _repository.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }

}