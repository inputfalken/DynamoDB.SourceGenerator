using System.IO;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Converters;

namespace DynamoDBGenerator.Internal.Converters;

internal sealed class MemoryStreamConverter : IReferenceTypeConverter<MemoryStream>
{
    public MemoryStream? Read(AttributeValue attributeValue)
    {
        return attributeValue.B;
    }

    public AttributeValue Write(MemoryStream element)
    {
        return new AttributeValue { B = element };
    }
}