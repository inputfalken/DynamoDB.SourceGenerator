using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<int>))]
public partial class IntTests : RecordMarshalAsserter<int>
{

    public IntTests() : base(new[] {int.MaxValue, 9323, 328382}, x => new AttributeValue {N = x.ToString()})
    {
    }
    protected override Container<int> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<int> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<int?>))]
public partial class NullableIntTests : RecordMarshalAsserter<int?>
{

    public NullableIntTests() : base(new int?[] {int.MaxValue, 9323, 328382, null}, x => x is null ? null : new AttributeValue {N = x.Value.ToString()})
    {
    }
    protected override Container<int?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<int?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}
