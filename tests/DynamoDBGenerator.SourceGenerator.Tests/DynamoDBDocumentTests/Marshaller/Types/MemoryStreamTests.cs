using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<MemoryStream>))]
public partial class MemoryStreamTests : NotNullRecordElementMarshalAsserter<MemoryStream>
{


    protected override Container<MemoryStream> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<MemoryStream> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public MemoryStreamTests() : base(new[] {new MemoryStream(new byte[] {12}), new MemoryStream(new byte[] {13})}, x => new AttributeValue {B = x})
    {
    }
}

[DynamoDBMarshaller(typeof(Container<MemoryStream?>))]
public partial class NullableMemoryStreamTests : RecordMarshalAsserter<MemoryStream?>
{


    protected override Container<MemoryStream?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<MemoryStream?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public NullableMemoryStreamTests() : base(new[] {new MemoryStream(new byte[] {12}), new MemoryStream(new byte[] {13})}, x => x is null ? null : new AttributeValue {B = x})
    {
    }
}