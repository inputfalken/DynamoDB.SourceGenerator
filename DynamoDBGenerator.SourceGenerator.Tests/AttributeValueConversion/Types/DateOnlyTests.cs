namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class DateOnlyTests
{
    [Fact]
    public void BuildAttributeValues_DateOnlyProperty_Included()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        var @class = new DateOnlyClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateOnlyClass.TimeStamp))
            .And
            .ContainSingle(x => DateOnly.Parse(x.Value.S) == timeStamp);
    }

    [Fact]
    public void BuildAttributeValues_DateOnlyProperty_FormatIsISO8601()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        var @class = new DateOnlyClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateOnlyClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
}

[AttributeValueGenerator]
public partial class DateOnlyClass
{
    [DynamoDBProperty]
    public DateOnly TimeStamp { get; set; }
}