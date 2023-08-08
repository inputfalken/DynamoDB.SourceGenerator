using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBDocument(typeof(DateOnlyClass))]
[DynamoDBDocument(typeof(DateTimeClass))]
[DynamoDBDocument(typeof(DateTimeOffsetClass))]
public partial class TemporalTests
{
    [Fact]
    public void Deserialize_DateOnlyClass_ShouldSucceed()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        DateOnlyClassDocument.Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DateOnlyClass.TimeStamp), new AttributeValue
                    {
                        S = timeStamp.ToString("O")
                    }
                }
            })
            .Should()
            .BeOfType<DateOnlyClass>()
            .Which
            .TimeStamp
            .Should()
            .Be(timeStamp);
    }

    [Fact]
    public void Deserialize_DateTimeClass_ShouldSucceed()
    {
        var timeStamp = DateTime.Now;

        DateTimeClassDocument.Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DateTimeClass.TimeStamp), new AttributeValue
                    {
                        S = timeStamp.ToString("O")
                    }
                }
            })
            .Should()
            .BeOfType<DateTimeClass>()
            .Which
            .TimeStamp
            .Should()
            .Be(timeStamp);
    }

    [Fact]
    public void Deserialize_DateTimeOffsetClass_ShouldSucceed()
    {
        var timeStamp = DateTimeOffset.Now;

        DateTimeOffsetClassDocument.Deserialize(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(DateTimeOffsetClass.TimeStamp), new AttributeValue
                    {
                        S = timeStamp.ToString("O")
                    }
                }
            })
            .Should()
            .BeOfType<DateTimeOffsetClass>()
            .Which
            .TimeStamp
            .Should()
            .Be(timeStamp);
    }

}

public class DateTimeOffsetClass
{
    public DateTimeOffset TimeStamp { get; set; }
}

public class DateTimeClass
{
    public DateTime TimeStamp { get; set; }
}

public class DateOnlyClass
{
    public DateOnly TimeStamp { get; set; }
}