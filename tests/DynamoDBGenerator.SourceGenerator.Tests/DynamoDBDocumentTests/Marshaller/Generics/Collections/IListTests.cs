using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<IList<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIListElementTests : NoneNullableElementAsserter<IList<string>, string>
{
    public NoneNullableIListElementTests() : base(Strings(),x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IList<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IList<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        TextMarshaller.Unmarshall(attributeValues).Rows.Should().BeOfType<List<string>>();
    }
}

[DynamoDBMarshaller(typeof(Text<IList<string?>>))]
public partial class NullableIListElementTests : NullableElementAsserter<IList<string?>, string?>
{
    public NullableIListElementTests() : base(Strings(),x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IList<string?>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IList<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        TextMarshaller.Unmarshall(attributeValues).Rows.Should().BeOfType<List<string?>>();
    }
}
