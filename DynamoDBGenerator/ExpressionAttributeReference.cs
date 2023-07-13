using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator;

public readonly record struct AttributeReference
{
    private readonly Lazy<string> _name;
    private readonly Lazy<string> _value;

    public AttributeReference(in Lazy<string> name, in Lazy<string> value)
    {
        _name = name;
        _value = value;
    }

    /// <summary>
    /// Dynamodb column reference.
    /// </summary>
    public string Name => _name.Value;

    /// <summary>
    /// update value provided in execution.
    /// </summary>
    public string Value => _value.Value;
}

public interface IDynamoDbDocument<TEntity, out TEntityReferences>
{
    /// <summary>
    /// Contains the keys for <see cref="TEntity"/>.
    /// </summary>
    public Dictionary<string, AttributeValue> Keys(TEntity entity);

    /// <summary>
    ///  Serializes the <typeparamref name="TEntity"/> into AttributeValues.
    /// </summary>
    public Dictionary<string, AttributeValue> Serialize(TEntity entity);

    /// <summary>
    /// Deserializes the provided AttributeValues into an <typeparamref name="TEntity"/>.
    /// </summary>
    public TEntity Deserialize(Dictionary<string, AttributeValue> document);

    /// <summary>
    ///  Creates a <see cref="AttributeExpression{T}"/> to build an 'UpdateExpression'.
    /// </summary>
    public AttributeExpression<TEntity> UpdateExpression(Func<TEntityReferences, string> updateExpressions);

    /// <summary>
    ///  Creates a <see cref="AttributeExpression{T}"/> to build an 'ConditionExpression'.
    /// </summary>
    public AttributeExpression<TEntity> ConditionExpression(Func<TEntityReferences, string> conditionalExpressions);
}

public class AttributeExpression<T>
{
    public AttributeExpression(IExpressionAttributeReferences<T> references, string expression)
    {
        References = references;
        Expression = expression;
    }

    public IExpressionAttributeReferences<T> References { get; }
    public string Expression { get; }
}

public interface IExpressionAttributeReferences<in TEntity>
{
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> whose elements are retrieved when the names have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> AccessedNames();

    /// <summary>
    /// An <see cref="IEnumerable{T}"/> whose elements are retrieved when the values have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(TEntity entity);
}