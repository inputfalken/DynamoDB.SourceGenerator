using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

/// <summary>
/// Represents a tracker for attribute values used in DynamoDB expression.
/// </summary>
/// <typeparam name="TArgument">The type of argument used in DynamoDB operations.</typeparam>
public interface IAttributeExpressionValueTracker<in TArgument>
{
    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the values have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, AttributeValue>> AccessedValues(TArgument arg);
}