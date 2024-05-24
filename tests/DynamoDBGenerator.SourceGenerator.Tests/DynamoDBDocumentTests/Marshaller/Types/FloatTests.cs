using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<float>))]
public partial class FloatTests : RecordMarshalAsserter<float>
{
    public FloatTests() : base(new[] {3_000.5F}, x => new() {N = x.ToString(CultureInfo.InvariantCulture)})
    {
    }

    protected override Container<float> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<float> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<float?>))]
public partial class NullableFloatTests : RecordMarshalAsserter<float?>
{
    public NullableFloatTests() : base(new float?[] {3_000.5F, null}, x => x is null ? null : new AttributeValue {N = x.Value.ToString(CultureInfo.InvariantCulture)})
    {
    }

    protected override Container<float?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<float?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}
