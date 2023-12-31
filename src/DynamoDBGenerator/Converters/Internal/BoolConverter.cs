using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class BoolConverter : IValueTypeConverter<bool>
{
    private static AttributeValue True { get; } = new() { BOOL = true };
    private static AttributeValue False { get; } = new() { BOOL = false };

    public bool? Read(AttributeValue attributeValue)
    {
        return attributeValue.IsBOOLSet ? attributeValue.BOOL : null;
    }

    public AttributeValue Write(bool element)
    {
        return element ? True : False;
    }
}