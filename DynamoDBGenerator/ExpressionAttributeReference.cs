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

public abstract class AttributeReferences<TEntity>
{
    public abstract IEnumerable<KeyValuePair<string, string>> ToExpressionAttributeNameEnumerable();
    public abstract IEnumerable<KeyValuePair<string, AttributeValue>> ToExpressionAttributeValueEnumerable(TEntity entity);
}