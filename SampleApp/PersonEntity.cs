using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

public partial class PersonEntity
{
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; } = null!;

    [DynamoDBHashKey]
    public string MyHashKey { get; set; } = null!;
    [DynamoDBRangeKey]
    public string MyRangeKey { get; set; } = null!;
}