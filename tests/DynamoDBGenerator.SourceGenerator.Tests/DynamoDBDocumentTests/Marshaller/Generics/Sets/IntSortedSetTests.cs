using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<SortedSet<int>>))]
public partial class IntSortedSetTests() : SetAsserter<SortedSet<int>, int>([2, 3, 4, 5], x => new SortedSet<int>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<SortedSet<int>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<SortedSet<int>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}