using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class PostalCode
{
    [DynamoDBProperty]
    public string ZipCode { get; set; }

    [DynamoDBProperty]
    public string Town { get; set; }
}