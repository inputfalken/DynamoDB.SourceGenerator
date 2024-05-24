using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<bool>))]
public partial class BoolTests : RecordMarshalAsserter<bool>
{
    public BoolTests() : base(new[] {true, false}, x => new AttributeValue {BOOL = x})
    {
    }
    protected override Container<bool> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<bool> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<bool?>))]
public partial class NullableBoolTests : RecordMarshalAsserter<bool?>
{
    public NullableBoolTests() : base(new[] {true, false}.Cast<bool?>().Append(null), x => x is null ? null : new AttributeValue {BOOL = x.Value})
    {
    }
    protected override Container<bool?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<bool?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}
