using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator ]
public partial class PersonEntity
{
    // TODO figure out why the KeyValuePair is not evaluated correctly.
    [DynamoDBProperty]
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; }

}