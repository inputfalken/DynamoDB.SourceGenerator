using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(EntityType = typeof(Container<HashSet<string>>))]
public partial class StringHashSetTests()
    : NoneNullableElementAsserter<HashSet<string>, string>(Strings(), x => new HashSet<string>(x))
{
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<HashSet<string>> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    protected override Container<HashSet<string>> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}