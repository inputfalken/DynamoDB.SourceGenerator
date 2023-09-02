using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBMarshaller(typeof(DateOnlyClass))]
[DynamoDBMarshaller(typeof(DateTimeClass))]
[DynamoDBMarshaller(typeof(DateTimeOffsetClass))]
public partial class TemporalTests
{
    [Fact]
    public void Deserialize_DateOnlyClass_ShouldSucceed()
    {
        var timeStamp = DateOnly.FromDateTime(DateTime.Now);

        DateOnlyClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
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

        DateTimeClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
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

        DateTimeOffsetClassMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
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