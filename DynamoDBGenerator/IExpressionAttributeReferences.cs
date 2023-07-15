using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

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