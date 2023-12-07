using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(NameList))]
public partial class IntHashSetTests : SetAsserter<HashSet<int>, int>
{

    public IntHashSetTests() : base(new[] {2, 3, 4, 5}, x => new HashSet<int>(x))
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(NameList element)
    {
        return NameListMarshaller.Marshall(element);
    }
    protected override NameList UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return NameListMarshaller.Unmarshall(attributeValues);
    }
}