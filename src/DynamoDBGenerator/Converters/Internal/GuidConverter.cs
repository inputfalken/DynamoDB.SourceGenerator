using System;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class GuidConverter : IValueTypeConverter<Guid>, IStaticSingleton<GuidConverter>
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