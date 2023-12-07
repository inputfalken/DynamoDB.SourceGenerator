using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(NameList))]
public partial class HashSetTests : NotNulStringSetAsserter<HashSet<string>>
{
    public HashSetTests() : base(x => new HashSet<string>(x))
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