using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class BoolConverter : IAttributeValueConverter<bool>
{
    private static AttributeValue True { get; } = new() { BOOL = true };
    private static AttributeValue False { get; } = new() { BOOL = false };

    public bool Read(AttributeValue attributeValue)
    {
        return attributeValue.BOOL;
    }

    public AttributeValue Write(bool element)
    {
        return element ? True : False;
    }
}