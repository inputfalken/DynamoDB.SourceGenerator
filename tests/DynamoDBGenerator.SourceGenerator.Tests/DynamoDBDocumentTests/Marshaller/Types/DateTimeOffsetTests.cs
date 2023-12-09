using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<DateTimeOffset>))]
public partial class DateTimeOffsetTests : RecordMarshalAsserter<DateTimeOffset>
{

    protected override Container<DateTimeOffset> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTimeOffset> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public DateTimeOffsetTests() : base(new[] {DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1)}, x => new AttributeValue {S = x.ToString("O")})
    {
    }
}

[DynamoDBMarshaller(typeof(Container<DateTimeOffset?>))]
public partial class NullableDateTimeOffsetTests : RecordMarshalAsserter<DateTimeOffset?>
{

    protected override Container<DateTimeOffset?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTimeOffset?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public NullableDateTimeOffsetTests() : base(new[] {DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1)}.Cast<DateTimeOffset?>().Append(null), x => x is null ? null : new AttributeValue {S = x.Value.ToString("O")})
    {
    }
}