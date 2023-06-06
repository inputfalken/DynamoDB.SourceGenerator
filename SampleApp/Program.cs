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


        var test = updateAddress.PostalCode.Address.PostalCode.Address.PostalCode.ZipCode.Name;
        var f=  updateAddress.ToDictionary(x => x.Key, x => x.Value);

        foreach (var keyValuePair in f)
        {
            Console.WriteLine(keyValuePair);
        }
    }
}

public class UpdateAddressStreet
{
    
}
[DynamoDBUpdateOperation(typeof(Address))]
public partial class Repository
{
    public Address_ExpressionAttribute.AddressAttributeReferences UpdateAddress(Address address)
    {
        return GetAddressAttributeReferences();
    }
}