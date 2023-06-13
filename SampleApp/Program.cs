using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        new Repository().UpdatePerson(new PersonEntity()
        {
            Id = "Abc",
            Firstname = "John",
            Lastname = "Doe",
            Address = new Address()
            {
                Id = "SomeId",
                PostalCode = new PostalCode()
                {
                    ZipCode = "123"
                },
                Street = "Abc"
            }
        });
    }
    
}

[DynamoDBUpdateOperation(typeof(PersonEntity))]
public partial class Repository
{
    public void UpdatePerson(PersonEntity person)
    {
        var attributeReferences =
            CreatePersonEntityUpdateItemRequest(person, "TableName", x => $"{x.Firstname.Name} = {x.Firstname.Value}");

        foreach (var keyValuePair in attributeReferences.Key)
            Console.WriteLine($"Keys: {keyValuePair}");

        foreach (var keyValuePair in attributeReferences.ExpressionAttributeValues)
            Console.WriteLine($"Values: {keyValuePair}");

        foreach (var keyValuePair in attributeReferences.ExpressionAttributeNames)
            Console.WriteLine($"Names: {keyValuePair}");

        Console.WriteLine($"UpdateExpression: {attributeReferences.UpdateExpression}");
    }
}