using System;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class GuidConverter : IValueTypeConverter<Guid>
{
    public Guid? Read(AttributeValue attributeValue)
    {
        return Guid.TryParse(attributeValue.S, out var guid) ? guid : null;
        
    }

    public AttributeValue Write(Guid element)
    {
        return new AttributeValue { S = element.ToString() };
    }
}