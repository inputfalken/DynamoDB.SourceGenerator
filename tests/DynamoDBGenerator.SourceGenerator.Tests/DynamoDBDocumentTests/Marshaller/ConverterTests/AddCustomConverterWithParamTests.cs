using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.ConverterTests;

[DynamoDbMarshallerOptions(Converters = typeof(Converter))]
[DynamoDBMarshaller(typeof(Container<Money>))]
public partial class AddCustomConverterWithParamTests : RecordMarshalAsserter<AddCustomConverterWithParamTests.Money>
{
    
    public class Converter : AttributeValueConverters
    {
        public Converter(MoneyConverter moneyConverter)
        {
            MoneyConverter = moneyConverter;
        }
        public MoneyConverter MoneyConverter { get; } 
    }

    public class MoneyConverter : IValueTypeConverter<Money>
    {
        public static AttributeValue WriteImplementation(Money element)
        {
            return new AttributeValue { S = element.ToString() };
        }
        public Money? Read(AttributeValue attributeValue)
        {
            return Money.Parse(attributeValue.S);
        }

        public AttributeValue Write(Money element)
        {
            return WriteImplementation(element);
        }
    }

    
    public readonly struct Money
    {
        public string Currency { get;  }
        public decimal Value { get;  }

        public Money(decimal value, string currency)
        {
            Value = value;
            Currency = currency;
        }


        public override string ToString()
        {
            return $"{Value}:{Currency}";
        }

        public static Money Parse(string input)
        {
            var index = input.IndexOf(':');
            var currency = input[(index + 1)..];
            var value = input[..index];
            return new Money(decimal.Parse(value), currency);
        }
    }

    public AddCustomConverterWithParamTests() : base(new []{new Money(32932, "SEK"), new Money(3923, "EURO")}, MoneyConverter.WriteImplementation)
    {
    }

    protected override Container<Money> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller(new MarshallerOptions(new Converter(new MoneyConverter()))).Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<Money> element)
    {
        return ContainerMarshaller(new MarshallerOptions(new Converter(new MoneyConverter()))).Marshall(element);
    }
}
