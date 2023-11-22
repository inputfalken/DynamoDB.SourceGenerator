using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

/// <summary>
/// Represents a marshaller responsible for converting objects of type <typeparamref name="TEntity"/> and <typeparamref name="TArgument"/>
/// into a <see cref="Dictionary{TKey,TValue}"/> containing AttributeValues.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be marshalled.</typeparam>
/// <typeparam name="TArgument">The type of argument used for marshalling.</typeparam>
/// <typeparam name="TEntityAttributeNameTracker">The type for tracking attribute names related to <typeparamref name="TEntity"/>.</typeparam>
/// <typeparam name="TArgumentAttributeValueTracker">The type for tracking argument attribute values related to <typeparamref name="TArgument"/>.</typeparam>
public interface IDynamoDBMarshaller<TEntity, in TArgument, out TEntityAttributeNameTracker, out TArgumentAttributeValueTracker>
    where TEntityAttributeNameTracker : IAttributeExpressionNameTracker
    where TArgumentAttributeValueTracker : IAttributeExpressionValueTracker<TArgument>
{
    /// <summary>
    /// Creates a tracker for managing argument attribute values of type <typeparamref name="TArgument"/> for DynamoDB operations.
    /// </summary>
    /// <returns>A tracker for argument attribute values.</returns>
    public TArgumentAttributeValueTracker AttributeExpressionValueTracker();

    /// <summary>
    /// Creates a tracker for managing attribute names related to <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>A tracker for attribute names.</returns>
    public TEntityAttributeNameTracker AttributeExpressionNameTracker();

    /// <summary>
    /// Marshals an object of type <typeparamref name="TEntity"/> into a collection of AttributeValues.
    /// </summary>
    /// <param name="entity">The entity to be serialized.</param>
    /// <returns>A Dictionary containing AttributeValues.</returns>
    public Dictionary<string, AttributeValue> Marshall(TEntity entity);

    /// <summary>
    /// Unmarshalls a collection of AttributeValues into an object of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="attributes">The AttributeValues to be deserialized.</param>
    /// <returns>An object of type <typeparamref name="TEntity"/>.</returns>
    public TEntity Unmarshall(Dictionary<string, AttributeValue> attributes);

    /// <inheritdoc cref="IDynamoDBKeyMarshaller"/>
    public IDynamoDBKeyMarshaller PrimaryKeyMarshaller { get; }

    /// <inheritdoc cref="IDynamoDBKeyMarshaller"/> 
    public IDynamoDBIndexKeyMarshaller IndexKeyMarshaller(string index);
}