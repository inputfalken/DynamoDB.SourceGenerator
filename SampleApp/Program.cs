using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DynamoDBGenerator;
using DynamoDBGenerator.Extensions;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<Marshalling>();
    }

}

[DynamoDBDocument(typeof(PersonEntity))]
[DynamoDBDocument(typeof(Address))]
public partial class Repository
{
    public Repository()
    {
    }
}

[SimpleJob]
[MemoryDiagnoser]
public class Marshalling
{
    private readonly Repository _repository;
    private readonly DynamoDBContext _context;
    private readonly DynamoDBOperationConfig _dynamoDbOperationConfig;
    private readonly PersonEntity _singleElement;

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
    }


    [Benchmark]
    public PutItemRequest AwsDocumentApi()
    {
        return new PutItemRequest("TABLE", _context.ToDocument(_singleElement, _dynamoDbOperationConfig).ToAttributeMap(_dynamoDbOperationConfig.Conversion));
    }

    [Benchmark]
    public PutItemRequest SourceGenerationDocumentApi()
    {
        return _repository.PersonEntityDocument.ToPutItemRequest(_singleElement, "TABLE");
    }

    [Benchmark]
    public PutItemRequest AwsDocumentApiWithConditionExpression()
    {
        return new PutItemRequest(
            "TABLE",
            _context.ToDocument(_singleElement, _dynamoDbOperationConfig).ToAttributeMap(_dynamoDbOperationConfig.Conversion)
        )
        {
            ConditionExpression = "#Id <> :p1",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":p1", new AttributeValue {S = _singleElement.Id}}
            }
        };
    }

    [Benchmark]
    public PutItemRequest SourceGenerationDocumentApiWithConditionExpression()
    {
        return _repository.PersonEntityDocument.ToPutItemRequest(_singleElement, "TABLE", x => $"{x.Id.Name} <> {x.Id.Value}");
    }
}