using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(Container<IReadOnlySet<string>>))]
public partial class IReadOnlySetTests : NoneNullableElementAsserter<IReadOnlySet<string>, string>
{
    public IReadOnlySetTests() : base(Strings(),x => new SortedSet<string>(x))
    {

    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlySet<string>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<IReadOnlySet<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeHashset()
    {
        var (_, attributeValues) = Arguments();

        ContainerMarshaller.Unmarshall(attributeValues).Element.Should().BeOfType<HashSet<string>>();
    }
}