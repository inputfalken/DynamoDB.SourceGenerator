using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<ushort>))]
public partial class UShortTests : RecordMarshalAsserter<ushort>
{
    public UShortTests() : base(new ushort[] { 12, 34, 455 }, x => new AttributeValue { N = x.ToString() })
    {
    }

    protected override Container<ushort> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ushort> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<ushort?>))]
public partial class NullableUShortTests : RecordMarshalAsserter<ushort?>
{
    public NullableUShortTests() : base(new ushort?[] { 12, 34, 455, null },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString() })
    {
    }

    protected override Container<ushort?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ushort?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}