using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Container<IList<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIListElementTests : NoneNullableElementAsserter<IList<string>, string>
{
    public NoneNullableIListElementTests() : base(Strings(),x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IList<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<IList<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        ContainerMarshaller.Unmarshall(attributeValues).Element.Should().BeOfType<List<string>>();
    }
}

[DynamoDBMarshaller(typeof(Container<IList<string?>>))]
public partial class NullableIListElementTests : NullableElementAsserter<IList<string?>, string?>
{
    public NullableIListElementTests() : base(Strings(),x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IList<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<IList<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        var (_, attributeValues) = Arguments();

        ContainerMarshaller.Unmarshall(attributeValues).Element.Should().BeOfType<List<string?>>();
    }
}
