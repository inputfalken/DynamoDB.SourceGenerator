using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(EntityType = typeof(Container<DateTime>))]
public partial class DateTimeTests : RecordMarshalAsserter<DateTime>
{
    public DateTimeTests() : base(new[] { DateTime.Now, DateTime.Now.AddDays(1) },
        x => new AttributeValue { S = x.ToString("O") })
    {
    }

    protected override Container<DateTime> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTime> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(EntityType = typeof(Container<DateTime?>))]
public partial class NullableDateTimeTests : RecordMarshalAsserter<DateTime?>
{
    public NullableDateTimeTests() : base(
        new[] { DateTime.Now, DateTime.Now.AddDays(1) }.Cast<DateTime?>().Append(null),
        x => x is null ? null : new AttributeValue { S = x.Value.ToString("O") })
    {
    }

    protected override Container<DateTime?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTime?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}