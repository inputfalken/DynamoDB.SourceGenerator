using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(typeof(Text<List<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableListElementTests : NoneNullableElementAsserter<List<string>, string>
{
    public NoneNullableListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<List<string>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<List<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }
}

[DynamoDBMarshaller(typeof(Text<List<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableListElementTests : NullableElementAsserter<List<string?>, string?>
{
    public NullableListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Text<List<string?>> text)
    {
        return TextMarshaller.Marshall(text);
    }
    protected override Text<List<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return TextMarshaller.Unmarshall(attributeValues);
    }
}