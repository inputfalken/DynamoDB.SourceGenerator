using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Switch))]
public partial class BoolTests
{
    private static readonly Switch Dto = new(true);
    private static readonly Dictionary<string, AttributeValue> AttributeValues = new() {{nameof(Switch.On), new AttributeValue {BOOL = true}}};


    [Fact]
    public void Marshall()
    {
        SwitchMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        SwitchMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);
    }


    public record Switch(bool On);
}