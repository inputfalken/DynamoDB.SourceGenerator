using System.Net.Mail;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.ConverterTests;

[DynamoDbMarshallerOptions(Converters = typeof(Converter))]
[DynamoDBMarshaller(typeof(Container<User>))]
public partial class AddCustomConverterTests : RecordMarshalAsserter<AddCustomConverterTests.User>
{
    

    public record User(Mail Email);

    public record Mail(MailAddress MailAddress);
    
    public class Converter : AttributeValueConverters
    {
        public MailConverter MailConverter { get; } = new();
    }

    public class MailConverter : IReferenceTypeConverter<Mail>
    {
        public static AttributeValue WriteImplementation(Mail element)
        {
            return new AttributeValue { S = element.MailAddress.Address };
        }
        public Mail Read(AttributeValue attributeValue)
        {
            return new Mail(new MailAddress(attributeValue.S));
        }

        public AttributeValue Write(Mail element)
        {
            return WriteImplementation(element);
        }
    }

    public AddCustomConverterTests() : base(new []{new User(new Mail(new MailAddress("something@domain.com")))}, x => MailConverter.WriteImplementation(x.Email))
    {
    }

    protected override Container<User> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<User> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}