using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(EntityType = typeof(Container<IEnumerable<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableEnumerableElementTests : NoneNullableElementAsserter<IEnumerable<string>, string>
{
    public NoneNullableEnumerableElementTests() : base(Strings(), x => x)
    {
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IEnumerable<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }

    protected override Container<IEnumerable<string>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Should_NotBeLoaded()
    {
        var (_, attributeValues) = CreateArguments(new[] { "Hello!" });

        var res = ContainerMarshaller.Unmarshall(attributeValues);

        res.Element.TryGetNonEnumeratedCount(out _).Should().BeFalse();
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<IEnumerable<string?>>))]
public partial class NullableEnumerableElementTests : NullableElementAsserter<IEnumerable<string?>, string?>
{
    public NullableEnumerableElementTests() : base(Strings(), x => x)
    {
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IEnumerable<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }

    protected override Container<IEnumerable<string?>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Should_NotBeLoaded()
    {
        var (_, attributeValues) = CreateArguments(new[] { "Hello!" });

        var res = ContainerMarshaller.Unmarshall(attributeValues);

        res.Element.TryGetNonEnumeratedCount(out _).Should().BeFalse();
    }
}