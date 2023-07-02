using Amazon.DynamoDBv2.Model;

namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class DateTimeOffsetTests
{
    [Fact]
    public void BuildAttributeValues_DateTimeOffsetProperty_Included()
    {
        var timeStamp = DateTimeOffset.Now;

        var @class = new DateTimeOffsetClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeOffsetClass.TimeStamp))
            .And
            .ContainSingle(x => DateTimeOffset.Parse(x.Value.S) == timeStamp);
    }

    [Fact]
    public void BuildAttributeValues_DateTimeOffsetProperty_FormatIsISO8601()
    {
        var timeStamp = DateTimeOffset.Now;

        var @class = new DateTimeOffsetClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeOffsetClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
}

[DynamoDbDocument]
public partial class DateTimeOffsetClass
{
    [DynamoDBProperty]
    public DateTimeOffset TimeStamp { get; set; }
}