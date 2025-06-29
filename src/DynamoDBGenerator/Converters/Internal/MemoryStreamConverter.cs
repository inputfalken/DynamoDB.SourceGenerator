using System.IO;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.Converters.Internal;

internal sealed class MemoryStreamConverter : IReferenceTypeConverter<MemoryStream>, IStaticSingleton<MemoryStreamConverter>
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