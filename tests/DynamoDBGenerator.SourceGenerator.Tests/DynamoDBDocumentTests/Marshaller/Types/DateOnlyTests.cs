using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Delivery))]
public partial class DateOnlyTests
{
    private static readonly Delivery Dto = new(new DateOnly(2023, 12, 06));

    private static readonly Dictionary<string, AttributeValue> AttributeValues = new()
    {
        {nameof(Delivery.Date), new AttributeValue {S = Dto.Date.ToString("O")}}
    };

    [Fact]
    public void Marshall()
    {
        DeliveryMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        DeliveryMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);
    }

    public record Delivery(DateOnly Date);
}