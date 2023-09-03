﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DynamoDBGenerator;
using DynamoDBGenerator.Extensions;
using static SampleApp.Repository.PersonEntityMarshallerImplementation;
using PutItemRequest = Amazon.DynamoDBv2.Model.PutItemRequest;

namespace SampleApp;

internal static class Program
{
    private static IDynamoDBClient<PersonEntity, PersonEntity, NN_PersonEntityName, NN_PersonEntityValue> _toDynamoDBClient;
    public static void Main()
    {
        _toDynamoDBClient = new Repository().PersonEntityMarshaller.ToDynamoDBClient("MyTable", new AmazonDynamoDBClient());
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
        _repository.PersonEntityMarshaller.ToUpdateItemRequest(new PersonEntity(), "", (x, y) => x.Keys(y.Id, y.Lastname), (x, y) => "");
        return _repository.PersonEntityMarshaller.ToPutItemRequest(_singleElement,  ReturnValue.NONE, "TABLE");
    }

    [Benchmark]
    public PersonEntity DeserializeBySourceGeneration()
    {
        return _repository.PersonEntityMarshaller.Unmarshall(_attributeValues);
    }

}