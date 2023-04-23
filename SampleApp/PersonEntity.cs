using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator, AttributeValueKeysGenerator]
public partial class PersonEntity
{
    // TODO figure out why the KeyValuePair is not evaluated correctly.
    [DynamoDBProperty]
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; }

    [DynamoDBHashKey]
    public string MyHashKey { get; set; }

    [DynamoDBRangeKey]
    public string MySortKey { get; set; }
}