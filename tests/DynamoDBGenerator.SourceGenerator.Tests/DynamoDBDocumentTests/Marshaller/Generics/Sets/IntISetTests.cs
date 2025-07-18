using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<ISet<int>>))]
public partial class IntISetTests() : SetAsserter<ISet<int>, int>([2, 3, 4, 5], x => new HashSet<int>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ISet<int>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<ISet<int>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}