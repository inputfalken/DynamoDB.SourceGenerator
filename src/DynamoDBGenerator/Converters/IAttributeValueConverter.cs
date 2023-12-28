using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.Converters;

public interface IReferenceTypeConverter<T> where T : class
{
    public T? Read(AttributeValue attributeValue);
    public AttributeValue Write(T element);
}

public interface IValueTypeConverter<T> where T : struct
{
    public T? Read(AttributeValue attributeValue);
    public AttributeValue Write(T element);
}
