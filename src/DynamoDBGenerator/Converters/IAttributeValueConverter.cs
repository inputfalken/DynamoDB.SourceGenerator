using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters;

public interface IAttributeValueConverter<T>
{
    public T Read(AttributeValue attributeValue);
    public AttributeValue Write(T element);
}