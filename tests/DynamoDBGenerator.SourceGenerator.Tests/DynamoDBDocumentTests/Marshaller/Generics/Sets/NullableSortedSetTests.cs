using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<SortedSet<string?>>))]
public partial class NullableSortedSetTests() : NullableElementAsserter<SortedSet<string?>, string>(Strings(), x => new SortedSet<string?>(x))
{
    protected override Container<SortedSet<string?>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<SortedSet<string?>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}