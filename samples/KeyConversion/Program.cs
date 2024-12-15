// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.Attributes;

// PrimaryKeyMarshaller is used to convert the keys obtained from the [DynamoDBHashKey] and [DynamoDBRangeKey] attributes.
var keyMarshaller = EntityDTO.KeyMarshallerSample.PrimaryKeyMarshaller;

// IndexKeyMarshaller requires an argument that is the index name so it can provide you with the correct conversion based on the indexes you may have.
// It works the same way for both LocalSecondaryIndex and GlobalSecondaryIndex attributes.
var GSIKeyMarshaller = EntityDTO.KeyMarshallerSample.IndexKeyMarshaller("GSI");
var LSIKeyMarshaller = EntityDTO.KeyMarshallerSample.IndexKeyMarshaller("LSI");

[DynamoDBMarshaller(AccessName = "KeyMarshallerSample")]
public partial class EntityDTO
{
    [DynamoDBHashKey("PK")]
    public string Id { get; set; }

    [DynamoDBRangeKey("RK")]
    public string RangeKey { get; set; }

    [DynamoDBLocalSecondaryIndexRangeKey("LSI")]
    public string SecondaryRangeKey { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("GSI")]
    public string GlobalSecondaryIndexId { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI")]
    public string GlobalSecondaryIndexRangeKey { get; set; }
}
