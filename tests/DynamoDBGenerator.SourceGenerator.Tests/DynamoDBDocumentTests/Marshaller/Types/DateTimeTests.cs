using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<DateTime>))]
public partial class DateTimeTests : RecordMarshalAsserter<DateTime, DateTime>
{

    protected override Container<DateTime> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTime> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public DateTimeTests() : base(DateTime.Now, x => new AttributeValue {S = x.ToString("O")}, x => x)
    {
    }
}