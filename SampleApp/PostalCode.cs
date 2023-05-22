using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class PostalCode
{
    public string ZipCode { get; set; } = null!;

    public string Town { get; set; } = null!;
    public KeyValuePair<string, int> String { get; set; }

    public PostalCode PostCodeNested { get; set; }
}