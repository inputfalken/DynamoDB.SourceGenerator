using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<IReadOnlySet<decimal>>))]
public partial class DecimalIReadOnlySetTests() : SetAsserter<IReadOnlySet<decimal>, decimal>([2032m, 0.323232932m, 0.9329392m], x => new HashSet<decimal>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<IReadOnlySet<decimal>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<IReadOnlySet<decimal>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}