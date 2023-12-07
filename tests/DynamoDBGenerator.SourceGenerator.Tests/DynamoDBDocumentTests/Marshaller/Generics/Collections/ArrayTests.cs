using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text))]
// ReSharper disable once UnusedType.Global
public partial class ArrayTests : NoneNullableElementAsserter<string[], string>
{

    public ArrayTests() : base(Strings(), x => x.ToArray())
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