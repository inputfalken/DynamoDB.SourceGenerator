using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<ISet<decimal?>>))]
public partial class NullableDecimalISetTests() : SetAsserter<ISet<decimal?>, decimal?>([2032m, 0.323232932m, 0.9329392m, null], x => new HashSet<decimal?>(x))
{
    protected override Container<ISet<decimal?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<ISet<decimal?>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}