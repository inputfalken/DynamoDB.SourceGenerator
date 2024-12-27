using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<decimal>))]
public partial class DecimalTests : RecordMarshalAsserter<decimal>
{
    public DecimalTests() : base(new[] { 30.32093290329m },
        x => new AttributeValue { N = x.ToString(CultureInfo.InvariantCulture) })
    {
    }

    protected override Container<decimal> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<decimal> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<decimal?>))]
public partial class NullableDecimalTests : RecordMarshalAsserter<decimal?>
{
    public NullableDecimalTests() : base(new decimal?[] { null, 30.32093290329m },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString(CultureInfo.InvariantCulture) })
    {
    }

    protected override Container<decimal?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<decimal?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}