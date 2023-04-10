namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class DateTimeTests
{
    [Fact]
    public void BuildAttributeValues_DateTimeProperty_Included()
    {
        var timeStamp = DateTime.Now;
        
        var @class = new DateTimeClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeClass.TimeStamp))
            .And
            .ContainSingle(x => DateTime.Parse((string) x.Value.S) == timeStamp);
    }
    
    [Fact]
    public void BuildAttributeValues_DateTimeProperty_FormatIsISO8601()
    {
        var timeStamp = DateTime.Now;
        
        var @class = new DateTimeClass
        {
            TimeStamp = timeStamp
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DateTimeClass.TimeStamp))
            .And
            .ContainSingle(x => x.Value.S == timeStamp.ToString("O"));
    }
}

[AttributeValueGenerator]
public partial class DateTimeClass
{
    [DynamoDBProperty]
    public DateTime TimeStamp { get; set; }
}