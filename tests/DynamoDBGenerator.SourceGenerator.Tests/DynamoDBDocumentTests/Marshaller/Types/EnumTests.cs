using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Week))]
public partial class EnumTests
{
    private static Week CreateDto(DayOfWeek dayOfWeek) => new(dayOfWeek);
    private static Dictionary<string, AttributeValue> CreateAttributeValues(DayOfWeek dayOfWeek) => new()
        {{nameof(Week.Day), new AttributeValue {N = ((int)dayOfWeek).ToString()}}};

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Marshall(DayOfWeek dayOfWeek)
    {
        WeekMarshaller.Marshall(CreateDto(dayOfWeek)).Should().BeEquivalentTo(CreateAttributeValues(dayOfWeek));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Unmarshall(DayOfWeek dayOfWeek)
    {
        WeekMarshaller.Unmarshall(CreateAttributeValues(dayOfWeek)).Should().BeEquivalentTo(CreateDto(dayOfWeek));
    }

    public record Week(DayOfWeek Day);
}