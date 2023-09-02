namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshallert(typeof(DateOnlyClass))]
public partial class DateOnlyTests
{

    [Fact]
    public void Serialize_DateOnlyProperty_FormatIsISO8601()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        var @class = new DateOnlyClass
        {
            TimeStamp = timeStamp
        };

        DateOnlyClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateOnlyClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
    [Fact]
    public void Serialize_DateOnlyProperty_Included()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        var @class = new DateOnlyClass
        {
            TimeStamp = timeStamp
        };

        DateOnlyClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateOnlyClass.TimeStamp))
            .And
            .ContainSingle(x => DateOnly.Parse(x.Value.S) == timeStamp);
    }
}

public class DateOnlyClass
{
    [DynamoDBProperty]
    public DateOnly TimeStamp { get; set; }
}
