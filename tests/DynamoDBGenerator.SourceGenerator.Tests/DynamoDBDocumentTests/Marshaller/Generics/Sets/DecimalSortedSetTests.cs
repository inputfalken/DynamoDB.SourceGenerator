using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<SortedSet<decimal>>))]
public partial class DecimalSortedSetTests() : SetAsserter<SortedSet<decimal>, decimal>([2032m, 0.323232932m, 0.9329392m], x => new SortedSet<decimal>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<SortedSet<decimal>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<SortedSet<decimal>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}