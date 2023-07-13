using DynamoDBGenerator;

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

    }

}


[DynamoDbDocumentProperty(typeof(PersonEntity))]
[DynamoDbDocumentProperty(typeof(Address))]
public partial class Repository
{
    public Repository()
    {
    }
}