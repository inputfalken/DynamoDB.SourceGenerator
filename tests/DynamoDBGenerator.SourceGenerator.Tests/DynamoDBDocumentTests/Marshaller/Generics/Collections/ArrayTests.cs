using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Generics.Collections;

[DynamoDBMarshaller(EntityType = typeof(Container<string[]>))]
// ReSharper disable once UnusedType.Global
public partial class NoneNullableArrayElementTests : NoneNullableElementAsserter<string[], string>
{

    public NoneNullableArrayElementTests() : base(Strings(), x => x.ToArray())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<string[]> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<string[]> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<string?[]>))]
// ReSharper disable once UnusedType.Global
public partial class NullableArrayElementTests : NullableElementAsserter<string?[], string?>
{
    public NullableArrayElementTests() : base(Strings(), x => x.ToArray())
    {
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<string?[]> text)
    {
        return ContainerMarshaller.Marshall(text);
    }
    protected override Container<string?[]> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
}
