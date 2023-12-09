using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<ICollection<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableICollectionElementTests : NoneNullableElementAsserter<ICollection<string>, string>
{
    public NoneNullableICollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<ICollection<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<ICollection<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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

[DynamoDBMarshaller(typeof(Text<ICollection<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableICollectionElementTests : NullableElementAsserter<ICollection<string?>, string?>
{
    public NullableICollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<ICollection<string?>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<ICollection<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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
