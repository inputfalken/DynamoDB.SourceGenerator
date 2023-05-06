using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator("Abc")]
public partial class PersonEntity
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; } = null!;

    [DynamoDBHashKey]
    public string MyHashKey { get; set; } = null!;

    public PersonEntity()
    {
    }
    public class Testing
    {
    }
}