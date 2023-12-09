using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<string[]>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableArrayElementTests : NoneNullableElementAsserter<string[], string>
{

    public NoneNullableArrayElementTests() : base(Strings(), x => x.ToArray())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<string[]> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<string[]> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }
}

// TODO support nullable Array elements