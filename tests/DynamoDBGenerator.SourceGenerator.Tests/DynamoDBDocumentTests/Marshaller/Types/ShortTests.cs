using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<short>))]
public partial class ShortTests : RecordMarshalAsserter<short>
{

    public ShortTests() : base(new short[] {12, 343, 22}, s => new AttributeValue() {N = s.ToString()})
    {
    }
    protected override Container<short> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<short> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(typeof(Container<short?>))]
public partial class NullableShortTests : RecordMarshalAsserter<short?>
{

    public NullableShortTests() : base(new short?[] {12, 343, 22, null}, s => s is null ? null : new AttributeValue {N = s.Value.ToString()})
    {
    }
    protected override Container<short?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<short?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}
