using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<Guid>))]
public partial class GuidTests : RecordMarshalAsserter<Guid>
{
    public GuidTests() : base(new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
        x => new AttributeValue { S = x.ToString() })
    {
    }

    protected override Container<Guid> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<Guid> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<Guid?>))]
public partial class NullableGuidTests : RecordMarshalAsserter<Guid?>
{
    public NullableGuidTests() : base(new Guid?[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null },
        x => x is null ? null : new AttributeValue { S = x.Value.ToString() })
    {
    }

    protected override Container<Guid?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<Guid?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}