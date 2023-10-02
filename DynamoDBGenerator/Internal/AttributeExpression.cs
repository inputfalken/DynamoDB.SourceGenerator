using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.Internal;

internal sealed record AttributeExpression(Dictionary<string, AttributeValue> Values, Dictionary<string, string> Names, IReadOnlyList<string> Expressions) : IAttributeExpression
{
    public Dictionary<string, AttributeValue> Values { get; } = Values;
    public Dictionary<string, string> Names { get; } = Names;
    public IReadOnlyList<string> Expressions { get; } = Expressions;
}