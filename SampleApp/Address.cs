using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

public partial class Address
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;

    [DynamoDBProperty]
    public string Street { get; set; } = null!;

    [DynamoDBProperty]

    public PostalCode PostalCode { get; set; } = null!;
}