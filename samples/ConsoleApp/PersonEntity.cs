using Amazon.DynamoDBv2.DataModel;
namespace SampleApp;

public class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBLocalSecondaryIndexRangeKey(new []{"FOo"})]
    public string Firstname { get; set; }
    [DynamoDBGlobalSecondaryIndexHashKey(new []{"abc"})]
    public string Lastname { get; set; }
    public Address Address { get; set; }
}