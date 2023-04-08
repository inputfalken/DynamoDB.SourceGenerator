using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class Address
{
    [DynamoDBHashKey]
    public string Id { get; set; }
    [DynamoDBProperty]
    public string Street { get; set; }

    [DynamoDBProperty]
    public PostalCode PostalCode { get; set; }
}