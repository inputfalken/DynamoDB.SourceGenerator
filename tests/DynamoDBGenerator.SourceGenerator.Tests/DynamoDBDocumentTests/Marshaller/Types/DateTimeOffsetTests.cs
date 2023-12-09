using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<DateTimeOffset>))]
public partial class DateTimeOffsetTests : RecordMarshalAsserter<DateTimeOffset, DateTimeOffset>
{

    protected override Container<DateTimeOffset> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTimeOffset> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public DateTimeOffsetTests() : base(DateTimeOffset.Now, x => new AttributeValue {S = x.ToString("O")}, x => x)
    {
    }

}