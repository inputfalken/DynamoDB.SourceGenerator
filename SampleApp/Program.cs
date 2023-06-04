// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var repository = new Repository();

        var updateAddress = repository.UpdateAddress(new Address()
        {
            Id = "123",
            PostalCode = new PostalCode()
            {
                ZipCode = "1337",
                Town = "WilleTown"
            },
            Street = "WilleStreet"
        });
    }
}

[DynamoDBUpdateOperation(typeof(Address))]
public partial class Repository
{
    public UpdateItemRequest UpdateAddress(Address address)
    {


        return null;


    }
}