using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

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

/// <inheritdoc cref="IDynamoDBKeyMarshaller"/>
public interface IDynamoDBIndexKeyMarshaller : IDynamoDBKeyMarshaller
{
    /// <summary>
    /// Gets the index associated with the marshalling process.
    /// </summary>
    public string Index { get; }
}