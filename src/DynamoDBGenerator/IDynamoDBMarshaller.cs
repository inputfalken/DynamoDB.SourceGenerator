using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

/// <summary>
/// Represents a marshaller responsible for converting objects of type <typeparamref name="TEntity"/> and <typeparamref name="TArg"/>
/// into a <see cref="Dictionary{TKey,TValue}"/> containing AttributeValues.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be marshalled.</typeparam>
/// <typeparam name="TArg">The type of argument used for marshalling.</typeparam>
/// <typeparam name="TEntityAttributeNameTracker">The type for tracking attribute names related to <typeparamref name="TEntity"/>.</typeparam>
/// <typeparam name="TArgumentAttributeValueTracker">The type for tracking argument attribute values related to <typeparamref name="TArg"/>.</typeparam>
public interface IDynamoDBMarshaller<TEntity, in TArg, out TEntityAttributeNameTracker, out TArgumentAttributeValueTracker> 
    where TEntityAttributeNameTracker : IExpressionAttributeNameTracker
    where TArgumentAttributeValueTracker : IExpressionAttributeValueTracker<TArg>
{
    /// <summary>
    /// Creates a tracker for managing argument attribute values of type <typeparamref name="TArg"/> for DynamoDB operations.
    /// </summary>
    /// <returns>A tracker for argument attribute values.</returns>
    public TArgumentAttributeValueTracker AttributeExpressionValueTracker();

    /// <summary>
    /// Creates a tracker for managing attribute names related to <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>A tracker for attribute names.</returns>
    public TEntityAttributeNameTracker AttributeNameExpressionTracker();

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

    public IDynamoDBKeyMarshaller PrimaryKeyMarshaller { get; }
    public IDynamoDBIndexKeyMarshaller IndexKeyMarshaller(string index);
}

/// <summary>
/// Represents a tracker for attribute values used in DynamoDB expression.
/// </summary>
/// <typeparam name="TArg">The type of argument used in DynamoDB operations.</typeparam>
public interface IExpressionAttributeValueTracker<in TArg>
{
    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the values have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(TArg arg);
}

/// <summary>
/// Represents a tracker for attribute names used in DynamoDB expression.
/// </summary>
public interface IExpressionAttributeNameTracker
{

    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the names have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> AccessedNames();
}