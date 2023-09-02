namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshallert(typeof(DateTimeOffsetClass))]
public partial class DateTimeOffsetTests
{

    [Fact]
    public void Serialize_DateTimeOffsetProperty_FormatIsISO8601()
    {
        var timeStamp = DateTimeOffset.Now;

        var @class = new DateTimeOffsetClass
        {
            TimeStamp = timeStamp
        };

        DateTimeOffsetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeOffsetClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
    [Fact]
    public void Serialize_DateTimeOffsetProperty_Included()
    {
        var timeStamp = DateTimeOffset.Now;

        var @class = new DateTimeOffsetClass
        {
            TimeStamp = timeStamp
        };

        DateTimeOffsetClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeOffsetClass.TimeStamp))
            .And
            .ContainSingle(x => DateTimeOffset.Parse(x.Value.S) == timeStamp);
    }
}

public class DateTimeOffsetClass
{
    [DynamoDBProperty]
    public DateTimeOffset TimeStamp { get; set; }
}
