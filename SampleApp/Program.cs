// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2.Model;

namespace SampleApp;

internal static class Program
{
    public static void Main()
    {
        var personEntity = new PersonEntity()
        {
            Id = "Robin",
            Friends = new List<PersonEntity>(){},
            CreatedAtDate = DateOnly.FromDateTime(DateTime.Now),
            UpdatedAt = DateTime.Now
        };


        new PutItemRequest()
        {
            Item = personEntity.BuildAttributeValues(),
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                {PersonEntity.AttributeValueKeys.Name, new AttributeValue(){S = "A"}}
            }
        };

    }
}