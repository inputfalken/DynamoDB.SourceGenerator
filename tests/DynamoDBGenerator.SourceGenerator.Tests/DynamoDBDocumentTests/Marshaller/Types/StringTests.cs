using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<string>))]
public partial class StringTests : NotNullRecordElementMarshalAsserter<string>
{
    public StringTests() : base(new[] { "A", "B", "Hey", "Foo", "Bar" }, s => new AttributeValue { S = s })
    {
    }

    protected override Container<string> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<string> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<string?>))]
public partial class NullableStringTests : RecordMarshalAsserter<string?>
{
    public NullableStringTests() : base(new[] { "A", "B", "Hey", "Foo", "Bar", null },
        s => s is null ? null : new AttributeValue { S = s })
    {
    }

    protected override Container<string?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<string?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}