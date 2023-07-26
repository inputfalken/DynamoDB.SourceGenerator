namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Types;

[DynamoDBGenerator.DynamoDBDocument(typeof(WeekDayClass))]
[DynamoDBGenerator.DynamoDBDocument(typeof(OptionalWeekDayClass))]
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
    public void Serialize_WeekDayClass_ShouldBeNumber(DayOfWeek dayOfWeek)
    {
        WeekDayClassDocument
            .Serialize(new WeekDayClass {DayOfWeek = dayOfWeek})
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(WeekDayClass.DayOfWeek));
                x.Value.N.Should().Be($"{(int)dayOfWeek}");
            });
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Serialize_OptionalWeekDayClass_ShouldBeNumber(DayOfWeek dayOfWeek)
    {
        OptionalWeekDayClassDocument
            .Serialize(new OptionalWeekDayClass {DayOfWeek = dayOfWeek})
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(WeekDayClass.DayOfWeek));
                x.Value.N.Should().Be($"{(int)dayOfWeek}");
            });
    }

    [Fact]
    public void Serialize_OptionalWeekDayClass_NoValueProvided()
    {
        OptionalWeekDayClassDocument
            .Serialize(new OptionalWeekDayClass())
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_OptionalWeekDayClass_ValueExplicitlySetToNull()
    {
        OptionalWeekDayClassDocument
            .Serialize(new OptionalWeekDayClass() {DayOfWeek = null})
            .Should()
            .BeEmpty();
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