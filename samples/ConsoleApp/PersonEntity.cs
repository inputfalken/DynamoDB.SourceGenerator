using Amazon.DynamoDBv2.DataModel;
namespace SampleApp;

public class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBProperty("Foo")]
    //[DynamoDBGlobalSecondaryIndexHashKey(new []{"foo", "bar"})]
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public Address Address { get; set; }
}