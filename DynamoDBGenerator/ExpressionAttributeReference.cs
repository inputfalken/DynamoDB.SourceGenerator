using System.Collections;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator;

public record AttributeReference(string Name, string Value)
{
    /// <summary>
    /// Dynamodb column reference.
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// update value provided in execution.
    /// </summary>
    public string Value { get; } = Value;
}

public interface IExpressionAttributeReferences<in TEntity>
{
    public IEnumerable<KeyValuePair<string, string>> AccessedAttributeNames();
    public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedAttributeValues(TEntity entity);
}