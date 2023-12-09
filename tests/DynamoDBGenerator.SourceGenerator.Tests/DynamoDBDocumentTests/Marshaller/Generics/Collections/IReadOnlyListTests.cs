using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<IReadOnlyList<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIReadOnlyListElementTests : NoneNullableElementAsserter<IReadOnlyList<string>, string>
{
    public NoneNullableIReadOnlyListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IReadOnlyList<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IReadOnlyList<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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

[DynamoDBMarshaller(typeof(Text<IReadOnlyList<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableIReadOnlyListElementTests : NullableElementAsserter<IReadOnlyList<string?>, string?>
{
    public NullableIReadOnlyListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IReadOnlyList<string?>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IReadOnlyList<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        TextMarshaller.Unmarshall(attributeValues).Rows.Should().BeOfType<string?[]>();
    }
}