using System.Collections;
using System.Collections.Generic;

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

public abstract class AttributeReferences : IEnumerable<KeyValuePair<string, string>>
{
    protected abstract IEnumerable<KeyValuePair<string, string>> ToExpressionAttributeNameEnumerable();
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return ToExpressionAttributeNameEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}