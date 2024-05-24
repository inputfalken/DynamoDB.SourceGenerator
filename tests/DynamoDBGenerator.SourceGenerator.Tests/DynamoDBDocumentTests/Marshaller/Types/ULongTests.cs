using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<ulong>))]
public partial class ULongTests : RecordMarshalAsserter<ulong>
{

    public ULongTests() : base(new ulong[] {ulong.MaxValue, 3232, 8382832838, 3283823828328}, x => new AttributeValue {N = x.ToString()})
    {
    }
    protected override Container<ulong> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ulong> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<ulong?>))]
public partial class NullableULongTests : RecordMarshalAsserter<ulong?>
{

    public NullableULongTests() : base(new ulong?[] {ulong.MaxValue, 3232, 8382832838, 3283823828328, null}, x => x is null ? null : new AttributeValue {N = x.Value.ToString()})
    {
    }
    protected override Container<ulong?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ulong?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}
