using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; }

    [DynamoDBProperty]
    public int Count { get; set; }

    [DynamoDBProperty]
    public long LongCount { get; set; }

    [DynamoDBProperty]
    public bool IsSaved { get; set; }

    [DynamoDBProperty]
    public bool? IsUpdated { get; set; }

    [DynamoDBProperty]
    public DateTime? CreatedAt { get; set; }

    [DynamoDBProperty]
    public Address Address { get; set; }

    [DynamoDBProperty]
    public string[] Ids { get; set; }

    [DynamoDBProperty]
    public int?[] MaybeIds { get; set; }
    [DynamoDBProperty]
    public IEnumerable<PersonEntity> Friends { get; set; }
}