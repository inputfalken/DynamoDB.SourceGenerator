using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<double>))]
public partial class DoubleTests : RecordMarshalAsserter<double>
{
    public DoubleTests() : base(new[] { 30.9328932, 30.9328933 },
        x => new AttributeValue { N = x.ToString(CultureInfo.InvariantCulture) })
    {
    }

    protected override Container<double> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<double> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<double?>))]
public partial class NullableDoubleTests : RecordMarshalAsserter<double?>
{
    public NullableDoubleTests() : base(new double?[] { null, 30.9328933 },
        x => x is null ? null : new AttributeValue { N = x.Value.ToString(CultureInfo.InvariantCulture) })
    {
    }

    protected override Container<double?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<double?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}