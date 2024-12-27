using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<long>))]
public partial class LongTests : RecordMarshalAsserter<long>
{
    public LongTests() : base(new[] { long.MaxValue, 32, 832832838283 }, x => new AttributeValue { N = x.ToString() })
    {
    }

    protected override Container<long> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<long> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<long?>))]
public partial class NullableLongTests : RecordMarshalAsserter<long?>
{
    public NullableLongTests() : base(new long?[] { long.MaxValue, 32, 832832838283, null },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString() })
    {
    }

    protected override Container<long?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<long?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}