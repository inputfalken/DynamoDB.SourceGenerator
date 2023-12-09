using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<IReadOnlyCollection<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIReadOnlyCollectionElementTests : NoneNullableElementAsserter<IReadOnlyCollection<string>, string>
{
    public NoneNullableIReadOnlyCollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IReadOnlyCollection<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IReadOnlyCollection<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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

[DynamoDBMarshaller(typeof(Text<IReadOnlyCollection<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableIReadOnlyCollectionElementTests : NullableElementAsserter<IReadOnlyCollection<string?>, string?>
{
    public NullableIReadOnlyCollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<IReadOnlyCollection<string?>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<IReadOnlyCollection<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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
