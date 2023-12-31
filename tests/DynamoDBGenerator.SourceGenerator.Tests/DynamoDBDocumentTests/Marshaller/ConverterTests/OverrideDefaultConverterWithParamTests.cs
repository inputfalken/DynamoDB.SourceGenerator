using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.ConverterTests;

[DynamoDbMarshallerOptions(Converters = typeof(Converters))]
[DynamoDBMarshaller(typeof(Container<DateTime>))]
public partial class OverrideDefaultConverterWithParamTests : RecordMarshalAsserter<DateTime>
{
    public OverrideDefaultConverterWithParamTests() :
        base(new[] { new DateTime(2023, 12, 31), new DateTime(2023, 12, 30) }, UnixEpochDateTimeConverter.WriteImplementation)
    {
    }

    protected override Container<DateTime> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller(new MarshallerOptions(new Converters(new UnixEpochDateTimeConverter())))
            .Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DateTime> element)
    {
        return ContainerMarshaller(new MarshallerOptions(new Converters(new UnixEpochDateTimeConverter())))
            .Marshall(element);
    }

    public class Converters : AttributeValueConverters
    {
        public Converters(UnixEpochDateTimeConverter converter)
        {
            DateTimeConverter = converter;
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