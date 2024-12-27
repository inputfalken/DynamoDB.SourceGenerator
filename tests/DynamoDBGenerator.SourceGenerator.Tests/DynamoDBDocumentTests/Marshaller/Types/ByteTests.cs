using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<byte>))]
public partial class ByteTests : RecordMarshalAsserter<byte>
{
    public ByteTests() : base(new byte[] { 134, 10 }, x => new AttributeValue { N = x.ToString() })
    {
    }

    protected override Container<byte> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<byte> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<byte?>))]
public partial class NullableByteTests : RecordMarshalAsserter<byte?>
{
    public NullableByteTests() : base(new byte?[] { 134, 10, null },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString() })
    {
    }

    protected override Container<byte?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<byte?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}