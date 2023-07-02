using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        new PersonEntity()
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
        };


        var repo = new Repository();
    }

}


[DynamoDBUpdateOperation(typeof(PersonEntity))]
public partial class Repository
{
    public Repository()
    {
    }
}


