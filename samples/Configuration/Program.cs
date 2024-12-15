// See https://aka.ms/new-console-template for more information
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Configuration;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Converters;
using DynamoDBGenerator.Options;

_ = OverriddenConverter.OverriddenConverterMarshaller;
_ = EnumBehaviour.EnumBehaviourMarshaller;

[DynamoDbMarshallerOptions(Converters = typeof(MyCustomConverters))]
[DynamoDBMarshaller]
public partial record OverriddenConverter([property: DynamoDBHashKey] string Id, DateTime Timestamp);

// Implement a converter, there's also an IReferenceTypeConverter available for ReferenceTypes.
public class UnixEpochDateTimeConverter : IValueTypeConverter<DateTime>
{
    public UnixEpochDateTimeConverter()
    {
    }

    // Convert the AttributeValue into a .NET type.
    public DateTime? Read(AttributeValue attributeValue)
    {
        return long.TryParse(attributeValue.N, out var epoch)
            ? DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime
            : null;
    }

    // Convert the .NET type into an AttributeValue.
    public AttributeValue Write(DateTime element)
    {
        return new AttributeValue { N = new DateTimeOffset(element).ToUnixTimeSeconds().ToString() };
    }
}

// Create a new Converters class
// You don't have to inherit from AttributeValueConverters if you do not want to use the default converters provided.
public class MyCustomConverters : AttributeValueConverters
{
    // If you take constructor parameters, the source generator will recongnize it and change the way you access it into an method.
    // It's recommended to call the method once and save it into a class member.
    public MyCustomConverters()
    {
        // Override the default behaviour.
        DateTimeConverter = new UnixEpochDateTimeConverter();
    }
    // You could add more converter DataMembers as fields or properties to add your own custom conversions.
}