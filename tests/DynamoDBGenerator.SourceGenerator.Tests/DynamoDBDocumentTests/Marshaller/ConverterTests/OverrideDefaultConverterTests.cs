using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.ConverterTests;

[DynamoDbMarshallerOptions(Converters = typeof(Converters))]
[DynamoDBMarshaller(EntityType = typeof(Container<DateTime>))]
public partial class OverrideDefaultConverterTests : RecordMarshalAsserter<DateTime>
{
    public OverrideDefaultConverterTests() :
        base(new[] { new DateTime(2023, 12, 31), new DateTime(2023, 12, 30) }, UnixEpochDateTimeConverter.WriteImplementation)
    {
    }

    protected override Container<DateTime> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTime> element)
    {
        return ContainerMarshaller.Marshall(element);
    }

    public class Converters : AttributeValueConverters
    {
        public Converters()
        {
            DateTimeConverter = new UnixEpochDateTimeConverter();
        }
    }

    public class UnixEpochDateTimeConverter : IValueTypeConverter<DateTime>
    {
        public static AttributeValue WriteImplementation(DateTime element)
        {
            return new AttributeValue { N = new DateTimeOffset(element).ToUnixTimeSeconds().ToString() };
        }

        public DateTime? Read(AttributeValue attributeValue)
        {
            return long.TryParse(attributeValue.N, out var epoch)
                ? DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime
                : null;
        }

        public AttributeValue Write(DateTime element)
        {
            return WriteImplementation(element);
        }
    }
}
