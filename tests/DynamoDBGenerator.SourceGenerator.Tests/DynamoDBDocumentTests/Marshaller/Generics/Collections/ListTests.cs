using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(EntityType = typeof(Container<List<string>>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableListElementTests : NoneNullableElementAsserter<List<string>, string>
{
    public NoneNullableListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<List<string>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<List<string>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<List<string?>>))]
// ReSharper disable once UnusedType.Global
public partial class NullableListElementTests : NullableElementAsserter<List<string?>, string?>
{
    public NullableListElementTests() : base(Strings(), x => x.ToList())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<List<string?>> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<List<string?>> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}
