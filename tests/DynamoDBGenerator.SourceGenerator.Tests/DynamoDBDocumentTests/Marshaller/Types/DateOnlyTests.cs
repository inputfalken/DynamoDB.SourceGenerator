using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container))]
public partial class DateOnlyTests : RecordMarshalAsserter<DateOnly, DateOnly>
{

    protected override Container UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public DateOnlyTests() : base(new DateOnly(2023, 12, 08), x => new AttributeValue {S = x.ToString("O")}, x => x)
    {
    }

}