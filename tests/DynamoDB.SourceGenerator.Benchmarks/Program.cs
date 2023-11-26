// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DynamoDB.SourceGenerator.Benchmarks.Models;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Extensions;

BenchmarkRunner.Run<Marshalling>();

[DynamoDBMarshaller(typeof(PersonEntity))]
public partial class Repository
{
}

[SimpleJob]
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
    public PutItemRequest PutByAws()
    {
        return new PutItemRequest("TABLE", _context.ToDocument(_singleElement, _dynamoDbOperationConfig).ToAttributeMap(_dynamoDbOperationConfig.Conversion));
    }

    [Benchmark]
    public PutItemRequest PutBySourceGeneration()
    {
        return _repository.PersonEntityMarshaller.ToPutItemRequest(_singleElement, ReturnValue.NONE, "TABLE");
    }

    [Benchmark]
    public PersonEntity DeserializeBySourceGeneration()
    {
        return _repository.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }

}