using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Documents;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DynamoDBGenerator;
using DynamoDBGenerator.Extensions;
using Document = Amazon.Runtime.Documents.Document;
using PutItemRequest = Amazon.DynamoDBv2.Model.PutItemRequest;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<Marshalling>();
    }

}

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
        return _repository.PersonEntityMarshaller.ToPutItemRequest(_singleElement, "TABLE");
    }

    [Benchmark]
    public PersonEntity DeserializeBySourceGeneration()
    {
        return _repository.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }

}