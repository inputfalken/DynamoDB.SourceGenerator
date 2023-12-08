using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container))]
public partial class StringTests : RecordMarshalAsserter<string, string>
{


    protected override Container UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("Hey")]
    [InlineData("Foo")]
    [InlineData("Bar")]
    public void Unmarshall_Various_Strings(string text)
    {
        var (element, attributeValues) = CreateArguments(text);

        UnmarshallImplementation(attributeValues).Should().BeEquivalentTo(element);
    }
    
    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("Hey")]
    [InlineData("Foo")]
    [InlineData("Bar")]
    public void Marshall_Various_Strings(string text)
    {
        var (element, attributeValues) = CreateArguments(text);

        MarshallImplementation(element).Should().BeEquivalentTo(attributeValues);
    }


    public StringTests() : base("Hello World", s => new AttributeValue() {S = s}, s => s)
    {
    }
}