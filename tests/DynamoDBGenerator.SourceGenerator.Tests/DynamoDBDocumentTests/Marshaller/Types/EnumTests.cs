using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class EnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public EnumTests() : base(Enum.GetValues<DayOfWeek>(), x => new AttributeValue {N = ((int)x).ToString()})
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

[DynamoDBMarshaller(typeof(Container<DayOfWeek?>))]
public partial class NullableEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableEnumTests() : base(Enum.GetValues<DayOfWeek>().Cast<DayOfWeek?>().Append(null), x => x is null ? null : new AttributeValue {N = ((int)x).ToString()})
    {
    }
    protected override Container<DayOfWeek?> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}