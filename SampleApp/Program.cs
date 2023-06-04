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
        
        foreach (var keyValuePair in updateAddress)
        {
            Console.WriteLine(keyValuePair);
        }

        foreach (var kv in updateAddress.PostalCode)
        {
            Console.WriteLine(kv);
        }
    }
}

[DynamoDBUpdateOperation(typeof(Address))]
public partial class Repository
{
    public Address_ExpressionAttribute.AddressExpressionAttribute UpdateAddress(Address address)
    {
        return GetAddressExpressionAttribute();
    }
}