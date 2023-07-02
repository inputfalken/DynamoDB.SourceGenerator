using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
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

// TODO figure out how to source generate this interface for the root element only. One approach is to introduce depth into the Conversion method and check whether we're in the first iteration of the recursion.
// By exposing this interface; we're able to extend it into multiple methods such as UpdateItemRequest, DeleteRequest etc which means we don't need to maintain those flows by source generation.
public interface IDynamoDbDocument<TEntity, out TEntityReferences>
{
    /// <summary>
    /// Contains the keys for <see cref="TEntity"/>.
    /// </summary>
    public Dictionary<string, AttributeValue> Keys(TEntity entity);

    /// <summary>
    ///  Contains all the AttributeValues for <see cref="TEntity"/>.
    /// </summary>
    public Dictionary<string, AttributeValue> Marshal(TEntity entity);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="updateExpressions"></param>
    /// <returns></returns>
    public AttributeExpression<TEntity> UpdateExpression(Func<TEntityReferences, string> updateExpressions);
    public AttributeExpression<TEntity> ConditionExpression(Func<TEntityReferences, string> conditionalExpressions);
}

public readonly struct AttributeExpression<T>
{
    public AttributeExpression(IExpressionAttributeReferences<T> expressionAttributeReferences, string expression)
    {
        ExpressionAttributeReferences = expressionAttributeReferences;
        Expression = expression;
    }
    
    public IExpressionAttributeReferences<T> ExpressionAttributeReferences { get; }
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