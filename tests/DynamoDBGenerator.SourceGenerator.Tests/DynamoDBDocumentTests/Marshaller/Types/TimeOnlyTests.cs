using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<TimeOnly>))]
public partial class TimeOnlyTests : RecordMarshalAsserter<TimeOnly>
{

    protected override Container<TimeOnly> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<TimeOnly> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public TimeOnlyTests() : base(new[] {new TimeOnly(22, 12, 08), new TimeOnly(21, 12, 09)}, x => new AttributeValue {S = x.ToString("O")})
    {
    }
}

[DynamoDBMarshaller(typeof(Container<TimeOnly?>))]
public partial class NullableTimeOnlyTests : RecordMarshalAsserter<TimeOnly?>
{

    protected override Container<TimeOnly?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<TimeOnly?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public NullableTimeOnlyTests() : base(new[] {new TimeOnly(22, 12, 08), new TimeOnly(21, 12, 09)}.Cast<TimeOnly?>().Append(null), x => x is null ? null : new AttributeValue {S = x.Value.ToString("O")})
    {
    }
}
