using System.Collections.Generic;
namespace DynamoDBGenerator;

/// <summary>
/// Represents a tracker for attribute names used in DynamoDB expression.
/// </summary>
public interface IAttributeExpressionNameTracker
{

    /// <summary>
    ///     An <see cref="IEnumerable{T}" /> whose elements are retrieved when the names have been programmatically accessed.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> AccessedNames();
}