using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

public interface IDynamoDBMarshaller<TEntity, in TArg, out TEntityAttributeNameTracker, out TArgumentAttributeValueTracker> : IDynamoDBKeyMarshaller
    where TEntityAttributeNameTracker : IExpressionAttributeNameTracker
    where TArgumentAttributeValueTracker : IExpressionAttributeValueTracker<TArg>
{
    public TArgumentAttributeValueTracker AttributeExpressionValueTracker();
    public TEntityAttributeNameTracker AttributeNameExpressionTracker();
    /// <summary>
    ///     Serializes the <typeparamref name="TEntity" /> into AttributeValues.
    /// </summary>
    public Dictionary<string, AttributeValue> Marshall(TEntity entity);
    /// <summary>
    ///     Deserializes the <paramref name="attributes" /> into an <typeparamref name="TEntity" />.
    /// </summary>
    public TEntity Unmarshall(Dictionary<string, AttributeValue> attributes);
}

public interface IExpressionAttributeValueTracker<in TArg>
{
    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the values have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(TArg arg);
}

public interface IExpressionAttributeNameTracker
{

    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the names have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> AccessedNames();
}