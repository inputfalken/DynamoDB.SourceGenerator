using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        new Repository().UpdateStreetAndZipCode(new PersonEntity()
        {
            Id = "Abc",
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
    public void UpdateStreetAndZipCode(PersonEntity address)
    {
        var attributeReferences = GetPersonEntityAttributeReferences();


        var zipcodeReference = attributeReferences.Address.PostalCode.ZipCode;
        var streetReference = attributeReferences.Address.Street;
        var updateExpression = $"{streetReference.Name} = {streetReference.Value} & {zipcodeReference.Name} = {zipcodeReference.Value}";

        var updateRequest = new UpdateItemRequest()
        {
            UpdateExpression = updateExpression,
            Key = UpdatePersonEntityAttributeValueKeys(address),
            ExpressionAttributeNames = attributeReferences.ToExpressionAttributeNameEnumerable()
                .ToDictionary(x => x.Key, x => x.Value),
            ExpressionAttributeValues = attributeReferences.ToExpressionAttributeValueEnumerable(address)
                .ToDictionary(x => x.Key, x => x.Value),
        };

        foreach (var val in updateRequest.ExpressionAttributeNames)
        {
            Console.WriteLine(val);
        }

        foreach (var val in updateRequest.ExpressionAttributeValues)
        {
            Console.WriteLine(val);
        }

        Console.WriteLine(updateExpression);
    }
}