using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(EntityType = typeof(Container<IReadOnlyList<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableIReadOnlyListElementTests : NoneNullableElementAsserter<IReadOnlyList<string>, string>
{
    public NoneNullableIReadOnlyListElementTests() : base(Strings(), x => x.ToList())
    {
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlyList<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }

    protected override Container<IReadOnlyList<string>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        Arguments().Should().AllSatisfy(x =>
            ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<string[]>());
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<IReadOnlyList<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableIReadOnlyListElementTests : NullableElementAsserter<IReadOnlyList<string?>, string?>
{
    public NullableIReadOnlyListElementTests() : base(Strings(), x => x.ToList())
    {
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlyList<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }

    protected override Container<IReadOnlyList<string?>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeList()
    {
        Arguments().Should().AllSatisfy(x =>
            ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<string?[]>());
    }
}