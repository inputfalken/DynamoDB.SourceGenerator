using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Switch))]
public partial class BoolTests
{
    private static readonly Switch Poco = new() {On = true};
    private static readonly Dictionary<string, AttributeValue> AttributeValues = new() {{nameof(Switch.On), new AttributeValue {BOOL = true}}};


    [Fact]
    public void Marshall()
    {
        SwitchMarshaller.Marshall(Poco).Should().BeEquivalentTo(AttributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        SwitchMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Poco);
    }


    public record Switch
    {
        public bool On { get; set; }
    }
}