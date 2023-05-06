using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

public partial class PostalCode
{
    [DynamoDBProperty]
    public string ZipCode { get; set; } = null!;

    [DynamoDBProperty]
    public string Town { get; set; } = null!;
}