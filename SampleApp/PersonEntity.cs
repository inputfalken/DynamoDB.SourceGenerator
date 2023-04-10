using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator, AttributeKeyGenerator]
public partial class PersonEntity
{
    [DynamoDBProperty]
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Id2 { get; set; }

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
    public DateTime? CreateAtTimeStamp { get; set; }

    [DynamoDBProperty]
    public DateOnly? CreatedAtDate { get; set; }

    [DynamoDBProperty]
    public DateTime? UpdatedAt { get; set; }

    [DynamoDBProperty]
    public Address Address { get; set; }

    [DynamoDBProperty]
    public string[] Ids { get; set; }

    [DynamoDBProperty]
    public int?[] MaybeIds { get; set; }

    [DynamoDBProperty]
    public List<PersonEntity> Friends { get; set; }
    
}