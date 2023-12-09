using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<sbyte>))]
public partial class SByteTests : RecordMarshalAsserter<sbyte>
{
    public SByteTests() : base(new sbyte[] {1, 3, 4, 5}, x => new AttributeValue {N = x.ToString()})
    {
    }
    protected override Container<sbyte> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<sbyte> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(typeof(Container<sbyte?>))]
public partial class NullableSByteTests : RecordMarshalAsserter<sbyte?>
{
    public NullableSByteTests() : base(new sbyte?[] {1, 3, 4, 5}.Append(null), x => x is null ? null : new AttributeValue {N = x.Value.ToString()})
    {
    }
    protected override Container<sbyte?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<sbyte?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}