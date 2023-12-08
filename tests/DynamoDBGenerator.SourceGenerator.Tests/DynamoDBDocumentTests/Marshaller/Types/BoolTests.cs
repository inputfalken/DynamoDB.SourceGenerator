using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container))]
public partial class BoolTests : RecordMarshalAsserter<bool, bool>
{
    public BoolTests() : base(true, x => new AttributeValue {BOOL = x}, x => x)
    {
    }
    protected override Container UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    [Fact]
    public void Unmarshall_False()
    {
        var (element, attributeValues) = CreateArguments(false);

        ContainerMarshaller.Unmarshall(attributeValues).Should().BeEquivalentTo(element);

    }

    [Fact]
    public void Marshall_False()
    {
        var (element, attributeValues) = CreateArguments(false);

        ContainerMarshaller.Marshall(element).Should().BeEquivalentTo(attributeValues);
    }
}