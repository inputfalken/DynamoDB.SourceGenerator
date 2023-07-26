namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Types;

[DynamoDBGenerator.DynamoDBDocument(typeof(DateTimeClass))]
public partial class DateTimeTests
{

    [Fact]
    public void Serialize_DateTimeProperty_FormatIsISO8601()
    {
        var timeStamp = DateTime.Now;

        var @class = new DateTimeClass
        {
            TimeStamp = timeStamp
        };

        DateTimeClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
    [Fact]
    public void Serialize_DateTimeProperty_Included()
    {
        var timeStamp = DateTime.Now;

        var @class = new DateTimeClass
        {
            TimeStamp = timeStamp
        };

        DateTimeClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeClass.TimeStamp))
            .And
            .ContainSingle(x => DateTime.Parse((string)x.Value.S) == timeStamp);
    }
}

public class DateTimeClass
{
    [DynamoDBProperty]
    public DateTime TimeStamp { get; set; }
}
