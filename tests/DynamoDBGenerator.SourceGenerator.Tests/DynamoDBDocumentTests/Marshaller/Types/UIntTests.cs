using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<uint>))]
public partial class UIntTests : RecordMarshalAsserter<uint>
{
    public UIntTests() : base(new uint[] { uint.MaxValue, 232, 933 }, x => new AttributeValue { N = x.ToString() })
    {
    }

    protected override Container<uint> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<uint> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<uint?>))]
public partial class NullableUIntTests : RecordMarshalAsserter<uint?>
{
    public NullableUIntTests() : base(new uint?[] { uint.MaxValue, 232, 933, null },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString() })
    {
    }

    protected override Container<uint?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<uint?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}