using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

public interface IDynamoDbDocument<TEntity, out TEntityReferences> where TEntityReferences : IExpressionAttributeReferences<TEntity>
{
    /// <summary>
    /// Creates <see cref="Dictionary{TKey,TValue}"/> from the fields attributed with <see cref="DynamoDBHashKeyAttribute"/> and <see cref="DynamoDBRangeKeyAttribute"/> from <see cref="TEntity"/>.
    /// </summary>
    public Dictionary<string, AttributeValue> Keys(TEntity entity);

    /// <summary>
    ///  Serializes the <typeparamref name="TEntity"/> into AttributeValues.
    /// </summary>
    public Dictionary<string, AttributeValue> Serialize(TEntity entity);

    /// <summary>
    /// Deserializes the <paramref name="attributes"/> into an <typeparamref name="TEntity"/>.
    /// </summary>
    public TEntity Deserialize(Dictionary<string, AttributeValue> attributes);

    /// <summary>
    /// Creates an object who will track accessed properties.
    /// </summary>
    public TEntityReferences ExpressionAttributeTracker();
}