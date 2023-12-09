using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Container<IReadOnlyCollection<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIReadOnlyCollectionElementTests : NoneNullableElementAsserter<IReadOnlyCollection<string>, string>
{
    public NoneNullableIReadOnlyCollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlyCollection<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<IReadOnlyCollection<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        Arguments().Should().AllSatisfy(x => ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<string[]>());
    }
}

[DynamoDBMarshaller(typeof(Container<IReadOnlyCollection<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableIReadOnlyCollectionElementTests : NullableElementAsserter<IReadOnlyCollection<string?>, string?>
{
    public NullableIReadOnlyCollectionElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlyCollection<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<IReadOnlyCollection<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        Arguments().Should().AllSatisfy(x => ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<string?[]>());
    }
}