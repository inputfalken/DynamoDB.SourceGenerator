using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text))]
// ReSharper disable once UnusedType.Global
public partial class ListTests : NoneNullableCollectionElementAsserter<List<string>>
{
    public ListTests() : base(x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }
}