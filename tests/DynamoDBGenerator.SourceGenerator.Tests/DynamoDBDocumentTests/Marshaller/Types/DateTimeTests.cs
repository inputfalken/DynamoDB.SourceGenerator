using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Metadata))]
public partial class DateTimeTests
{
    private static readonly Metadata Dto = new(DateTime.Now);

    private static readonly Dictionary<string, AttributeValue> AttributeValues = new()
    {
        {nameof(Metadata.TimeStamp), new AttributeValue {S = Dto.TimeStamp.ToString("O")}}
    };

    [Fact]
    public void Marshall()
    {
        MetadataMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);
    }
    
    [Fact]
    public void Unmarshall()
    {
        MetadataMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);
    }

    public record Metadata(DateTime TimeStamp);
}