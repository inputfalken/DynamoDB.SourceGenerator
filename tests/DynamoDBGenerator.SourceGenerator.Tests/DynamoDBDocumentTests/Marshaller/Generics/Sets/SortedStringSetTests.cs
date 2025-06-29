using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<SortedSet<string>>))]
public partial class SortedStringSetTests() : NoneNullableElementAsserter<SortedSet<string>, string>(Strings(), x => new SortedSet<string>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<SortedSet<string>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<SortedSet<string>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeHashset()
    {
        Arguments().Should().AllSatisfy(x =>
            ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<SortedSet<string>>());
    }
}