using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

// TODO might be good to include a option to Get the keys based on a generic argument which would be TArg from IDynamoDBMarshaller. Then the consumer could customize their DynamoDBKeyAttributes on the TArg payload. This would not work for tuples though. 😢
/// <summary>
/// Represents a marshaller responsible for converting .NET based types into a format suitable for DynamoDB.
/// </summary>
public interface IDynamoDBKeyMarshaller
{
    /// <summary>
    /// Marshalls both the <paramref name="partitionKey"/> and <paramref name="rangeKey"/> into AttributeValues
    /// </summary>
    /// <param name="partitionKey">The partition key to be marshalled.</param>
    /// <param name="rangeKey">The range key to be marshalled.</param>
    /// <returns>A Dictionary containing marshalled AttributeValues for both keys.</returns>
    public Dictionary<string, AttributeValue> Keys(object partitionKey, object rangeKey);

    /// <summary>
    /// Marshalls a partition key into AttributeValues
    /// </summary>
    /// <param name="key">The partition key to be marshalled.</param>
    /// <returns>A Dictionary containing marshalled AttributeValues for the partition key.</returns>
    public Dictionary<string, AttributeValue> PartitionKey(object key);

    /// <summary>
    /// Marshalls a range key into AttributeValues
    /// </summary>
    /// <param name="key">The range key to be marshalled.</param>
    /// <returns>A Dictionary containing marshalled AttributeValues for the range key.</returns>
    public Dictionary<string, AttributeValue> RangeKey(object key);
}

public interface IDynamoDBIndexKeyMarshaller : IDynamoDBKeyMarshaller
{
    public string Index { get; }
}