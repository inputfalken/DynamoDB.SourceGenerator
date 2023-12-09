using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Container<ICollection<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableICollectionElementTests : NoneNullableElementAsserter<ICollection<string>, string>
{
    public NoneNullableICollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ICollection<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<ICollection<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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

[DynamoDBMarshaller(typeof(Container<ICollection<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableICollectionElementTests : NullableElementAsserter<ICollection<string?>, string?>
{
    public NullableICollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ICollection<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<ICollection<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
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
