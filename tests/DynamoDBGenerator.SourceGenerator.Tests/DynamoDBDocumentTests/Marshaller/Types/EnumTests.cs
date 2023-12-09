using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class EnumTests : RecordMarshalAsserter<DayOfWeek, DayOfWeek>
{

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Marshall_All_Days(DayOfWeek dayOfWeek)
    {
        var (element, attributeValues) = CreateArguments(dayOfWeek);
        ContainerMarshaller.Marshall(element).Should().BeEquivalentTo(attributeValues);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void Unmarshall_All_Days(DayOfWeek dayOfWeek)
    {
        var (element, attributeValues) = CreateArguments(dayOfWeek);
        ContainerMarshaller.Unmarshall(attributeValues).Should().BeEquivalentTo(element);
    }

    public record Week(DayOfWeek Day);

    public EnumTests() : base(DayOfWeek.Sunday, x => new AttributeValue {N = ((int)x).ToString()}, x => x)
    {
    }
    protected override Container<DayOfWeek> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}