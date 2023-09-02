using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBMarshaller(typeof(WeekDayClass))]
[DynamoDBMarshaller(typeof(OptionalWeekDayClass))]
public partial class EnumTests
{
    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Deserialize_WeekDayClass_ShouldBeNumber(DayOfWeek dayOfWeek)
    {
        WeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(WeekDayClass.DayOfWeek), new AttributeValue {N = ((int)dayOfWeek).ToString()}}})
            .Should()
            .BeOfType<WeekDayClass>()
            .Which
            .DayOfWeek
            .Should()
            .Be(dayOfWeek);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Deserialize_OptionalWeekDayClass_ShouldBeNumber(DayOfWeek dayOfWeek)
    {
        OptionalWeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(WeekDayClass.DayOfWeek), new AttributeValue {N = ((int)dayOfWeek).ToString()}}})
            .Should()
            .BeOfType<OptionalWeekDayClass>()
            .Which
            .DayOfWeek
            .Should()
            .Be(dayOfWeek);
    }

    [Fact]
    public void Deserialize_WeekDayClass_NoKeyValue()
    {
        var act = () => WeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>())
            .Should();

        act.Should().Throw<ArgumentNullException>();

    }

    [Fact]
    public void Deserialize_WeekDayClass_NoValueProvided()
    {
        var act = () => WeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(WeekDayClass.DayOfWeek), new AttributeValue {N = null}}})
            .Should();

        act.Should().Throw<ArgumentNullException>();

    }
    [Fact]
    public void Deserialize_OptionalWeekDayClass_NoValueProvided()
    {
        OptionalWeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue>())
            .Should()
            .BeOfType<OptionalWeekDayClass>()
            .Which
            .DayOfWeek
            .Should()
            .BeNull();
    }

    [Fact]
    public void Deserialize_OptionalWeekDayClass_ValueExplicitlySetToNull()
    {
        OptionalWeekDayClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(OptionalWeekDayClass.DayOfWeek), new AttributeValue {N = null}}})
            .Should()
            .BeOfType<OptionalWeekDayClass>()
            .Which
            .DayOfWeek.Should()
            .BeNull();
    }

}

public class OptionalWeekDayClass
{
    public DayOfWeek? DayOfWeek { get; set; }
}

public class WeekDayClass
{
    public DayOfWeek DayOfWeek { get; set; }
}