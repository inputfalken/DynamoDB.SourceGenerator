using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueKeysGenerator]
public partial class PersonEntity
{
    // TODO figure out why the KeyValuePair is not evaluated correctly.
    [DynamoDBProperty]
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; } = null!;

    [DynamoDBHashKey]
    public string MyHashKey { get; set; } = null!;
}
