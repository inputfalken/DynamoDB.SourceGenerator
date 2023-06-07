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

public interface IDynamoDbOperation<in TEntity>
{
    public Dictionary<string, string> BuildAttributeNames();
    public Dictionary<string, AttributeValue> BuildAttributeValues(TEntity entity);
    public Dictionary<string, AttributeValue> BuildKeys(TEntity entity);

    public string Expression { get; }
}

public interface IAttributeReferences<in TEntity>
{
    public IEnumerable<KeyValuePair<string, string>> ToExpressionAttributeNameEnumerable();
    public IEnumerable<KeyValuePair<string, AttributeValue>> ToExpressionAttributeValueEnumerable(TEntity entity);
}