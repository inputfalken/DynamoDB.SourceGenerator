using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;
using DynamoDBGenerator.Extensions;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var person = new PersonEntity()
        {
            Id = "Abc",
            Firstname = "John",
            Lastname = "Doe",
            Address = new Address()
            {
                Id = "SomeId",
                PostalCode = new PostalCode()
                {
                    ZipCode = "123",
                    Town = "Abc"
                },
                Street = "Abc",
                Neighbours = Array.Empty<PersonEntity>()
            }

        };

        var repo = new Repository();

        for (int i = 0; i < 100; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            var update = repo.PersonEntityDocument.CreatePutItemRequest(person, BuildConditionExpression);
            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Restart();
        }
    }
    private static string BuildConditionExpression(Repository.PersonEntity_Document.PersonEntityReferences x)
    {
        return $"{x.Address.PostalCode.Town.Name} == 'Stockholm'";
    }

}

public class Person
{
    public string FirstName { get; set; }
    public IReadOnlyList<Person> Friends { get; set; }
}

[DynamoDbDocumentProperty(typeof(PersonEntity))]
[DynamoDbDocumentProperty(typeof(Address))]
public partial class Repository
{
    public Repository()
    {
    }
}