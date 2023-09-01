using System.Collections;
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
        var personDocument = new Testing_1();
        new Testing_2().ToPutItemRequest(new PersonEntity(), "", (db, argument) => db.Address.Street);
        var setAddress = personDocument.ToUpdateItemRequest(
            new Address(),
            "",
            (db, argument) => $"{db.IsResident} = true AND {argument.Name} IS NOT NULL",
            (db, argument) => $"SET {db.Address.Street} = {argument.Name}"
        );
        UpdateItemResponse response = null;

        personDocument.Deserialize(response.Attributes);

        BenchmarkRunner.Run<Marshalling>();
    }

}

[DynamoDBDocument(typeof(PersonEntity))]
[DynamoDBDocument(typeof(Address))]
public partial class Repository
{
}

public class Testing_2 : IDynamoDBDocument<PersonEntity, PersonEntity, Testing_2.PersonNameTracker, Testing_2.PersonValueTracker>
{

    public class PersonNameTracker : IExpressionAttributeNameTracker
    {

        public Address Address { get; set; }
        public string IsResident { get; set; }
        public IEnumerable<KeyValuePair<string, string>> AccessedNames()
        {
            throw new NotImplementedException();
        }
    }
    public class PersonValueTracker : IExpressionAttributeValueTracker<PersonEntity>
    {

        public Address Address { get; set; }
        public string IsResident { get; set; }
        public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(PersonEntity arg)
        {
            throw new NotImplementedException();
        }
    }

    public Dictionary<string, AttributeValue> Keys(PersonEntity entity)
    {

        throw new NotImplementedException();
    }
    public Dictionary<string, AttributeValue> Serialize(PersonEntity entity)
    {
        throw new NotImplementedException();
    }
    public PersonEntity Deserialize(Dictionary<string, AttributeValue> attributes)
    {
        throw new NotImplementedException();
    }
    public PersonNameTracker AttributeNameExpressionTracker()
    {
        throw new NotImplementedException();
    }
    public AddressValueTracker AttributeExpressionValueTracker()
    {
        throw new NotImplementedException();
    }
}

public class Testing_1 : IDynamoDBDocument<PersonEntity, Address, Testing_1.PersonNameTracker, Testing_1.AddressValueTracker>
{
    public class AddressValueTracker : IExpressionAttributeValueTracker<Address>
    {

        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(Address arg)
        {
            throw new NotImplementedException();
        }
    }

    public class PersonNameTracker : IExpressionAttributeNameTracker
    {

        public Address Address { get; set; }
        public string IsResident { get; set; }
        public IEnumerable<KeyValuePair<string, string>> AccessedNames()
        {
            throw new NotImplementedException();
        }
    }

    public Dictionary<string, AttributeValue> Keys(PersonEntity entity)
    {

        throw new NotImplementedException();
    }
    public Dictionary<string, AttributeValue> Serialize(PersonEntity entity)
    {
        throw new NotImplementedException();
    }
    public PersonEntity Deserialize(Dictionary<string, AttributeValue> attributes)
    {
        throw new NotImplementedException();
    }
    public PersonNameTracker AttributeNameExpressionTracker()
    {
        throw new NotImplementedException();
    }
    public AddressValueTracker AttributeExpressionValueTracker()
    {
        throw new NotImplementedException();
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
        return _repository.PersonEntityDocument.ToPutItemRequest(_singleElement, "TABLE");
    }

    [Benchmark]
    public PersonEntity DeserializeBySourceGeneration()
    {
        return _repository.PersonEntityDocument.Deserialize(_attributeValues);
    }

}