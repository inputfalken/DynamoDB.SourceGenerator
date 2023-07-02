using System.Diagnostics;
using System.Runtime.CompilerServices;
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
                Street = "Abc"
            }

        };

        var repo = new Repository();

        for (int i = 0; i < 100; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            var update = repo.PersonEntityDocument.CreatePutItemRequest(person, x => x.Firstname.Value);
            Console.WriteLine(stopwatch.Elapsed);
            stopwatch.Restart();
        }
    }

}

[DynamoDBUpdateOperation(typeof(PersonEntity))]
public partial class Repository
{
    public Repository()
    {
    }
}