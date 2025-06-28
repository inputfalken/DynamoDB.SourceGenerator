using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<ISet<string?>>))]
public partial class NullableStringISetTests() : NullableElementAsserter<ISet<string?>, string>(Strings(), x => new HashSet<string?>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ISet<string?>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<ISet<string?>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_Implementation_ShouldBeHashset()
    {
        Arguments().Should().AllSatisfy(x =>
            ContainerMarshaller.Unmarshall(x.attributeValues).Element.Should().BeOfType<HashSet<string?>>());
    }
}