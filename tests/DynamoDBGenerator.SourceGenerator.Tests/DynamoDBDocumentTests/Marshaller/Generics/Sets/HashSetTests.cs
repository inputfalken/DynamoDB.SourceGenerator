using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Sets;

[DynamoDBMarshaller(typeof(SetDto))]
public partial class HashSetTests : NotNulStringSetAsserter<HashSet<string>>
{
    public HashSetTests() : base(x => new HashSet<string>(x))
    {

    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(SetDto element)
    {
        return SetDtoMarshaller.Marshall(element);
    }
    protected override SetDto UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return SetDtoMarshaller.Unmarshall(attributeValues);
    }
}