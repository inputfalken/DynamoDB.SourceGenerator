using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text))]
// ReSharper disable once UnusedType.Global
public partial class ReadOnlyNoneNullableElementTests : NoneNullableElementAsserter<IReadOnlyCollection<string>, string>
{
    public ReadOnlyNoneNullableElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        TextMarshaller.Unmarshall(attributeValues).Rows.Should().BeOfType<string[]>();
    }
}