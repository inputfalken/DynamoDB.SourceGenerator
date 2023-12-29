using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class CharConverter : IValueTypeConverter<char>
{
    public char? Read(AttributeValue attributeValue)
    {
        return attributeValue.S.Length > 0 ? attributeValue.S[0] : null;
    }

    public AttributeValue Write(char element)
    {
        return new AttributeValue { S = element.ToString() };
    }
}