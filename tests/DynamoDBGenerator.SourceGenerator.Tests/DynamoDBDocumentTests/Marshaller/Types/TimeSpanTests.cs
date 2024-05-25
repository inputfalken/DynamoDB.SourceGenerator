using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<TimeSpan>))]
public partial class TimeSpanTests : RecordMarshalAsserter<TimeSpan>
{
    public TimeSpanTests() : base(new []{TimeSpan.FromDays(1)}, _ => new() {S = "P1D"})
    {
    }

    protected override Container<TimeSpan> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<TimeSpan> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

}

[DynamoDBMarshaller(EntityType = typeof(Container<TimeSpan?>))]
public partial class NullableTimeSpanTests : RecordMarshalAsserter<TimeSpan?>
{
    public NullableTimeSpanTests() : base(new TimeSpan?[]{TimeSpan.FromDays(1), null}, x => x is not null ? new() {S = "P1D"} : null)
    {
    }

    protected override Container<TimeSpan?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<TimeSpan?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

}
