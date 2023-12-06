using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Decimal))]
[DynamoDBMarshaller(typeof(Double))]
[DynamoDBMarshaller(typeof(Float))]
public partial class DecimalTests
{
    private static readonly Decimal DecimalDto = new(30.32093290329m);

    private static readonly Dictionary<string, AttributeValue> DecimalAttributeValues = new()
    {
        {nameof(Decimal.DecimalValue), new AttributeValue {N = DecimalDto.DecimalValue.ToString()}}
    };

    [Fact]
    public void Marshal_Decimal()
    {
        DecimalMarshaller.Marshall(DecimalDto).Should().BeEquivalentTo(DecimalAttributeValues);
    }

    [Fact]
    public void Unmarshal_Decimal()
    {
        DecimalMarshaller.Unmarshall(DecimalAttributeValues).Should().BeEquivalentTo(DecimalDto);
    }

    public record Decimal(decimal DecimalValue);
    
    private static readonly Double DoubleDto = new(30.9328932);

    private static readonly Dictionary<string, AttributeValue> DoubleAttributeValues = new()
    {
        {nameof(Double.DoubleValue), new AttributeValue {N = DoubleDto.DoubleValue.ToString()}}
    };

    [Fact]
    public void Marshal_Double()
    {
        DoubleMarshaller.Marshall(DoubleDto).Should().BeEquivalentTo(DoubleAttributeValues);
    }

    [Fact]
    public void Unmarshal_Double()
    {
        DoubleMarshaller.Unmarshall(DoubleAttributeValues).Should().BeEquivalentTo(DoubleDto);
    }

    public record Double(double DoubleValue);
    
    private static readonly Float FloatDto = new(3_000.5F);

    private static readonly Dictionary<string, AttributeValue> FloatAttributeValues = new()
    {
        {nameof(Float.FloatValue), new AttributeValue {N = FloatDto.FloatValue.ToString()}}
    };

    [Fact]
    public void Marshal_Float()
    {
        FloatMarshaller.Marshall(FloatDto).Should().BeEquivalentTo(FloatAttributeValues);
    }

    [Fact]
    public void Unmarshal_Float()
    {
        FloatMarshaller.Unmarshall(FloatAttributeValues).Should().BeEquivalentTo(FloatDto);
    }

    public record Float(float FloatValue);

}