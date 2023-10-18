using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator;

/// <summary>
/// Contains the necessary fields to create AttributeExpressions.
/// </summary>
public interface IAttributeExpression
{
    /// <summary>
    /// Represents Name and values sent to DynamoDB.
    /// </summary>
    public Dictionary<string, AttributeValue> Values { get; }

    /// <summary>
    /// Represents the names of attributes in DynamoDB.
    /// </summary>
    public Dictionary<string, string> Names { get; }

    /// <summary>
    /// Represents the expressions.
    /// </summary>
    public IReadOnlyList<string> Expressions { get; }
}