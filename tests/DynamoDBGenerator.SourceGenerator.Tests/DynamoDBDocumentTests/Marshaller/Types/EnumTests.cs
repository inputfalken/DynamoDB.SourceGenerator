using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.String)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class StringEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public StringEnumTests() : base(new[] { DayOfWeek.Monday, DayOfWeek.Friday },
        x => new AttributeValue { S = x.ToString() })
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

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.String)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek?>))]
public partial class NullableStringEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableStringEnumTests() : base(new DayOfWeek?[] { DayOfWeek.Monday, DayOfWeek.Friday, null },
        x => x is not null ? new AttributeValue { S = x.ToString() } : null)
    {
    }

    protected override Container<DayOfWeek?> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.StringCI)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class StringCIEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public StringCIEnumTests() : base(new[] { DayOfWeek.Monday, DayOfWeek.Friday },
        x => new AttributeValue { S = x.ToString() })
    {
    }

    protected override Container<DayOfWeek> UnmarshallImplementation(Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_EnumIs_CaseInsensitive()
    {
        ContainerMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(Container<DayOfWeek>.Element),
                    new AttributeValue { S = DayOfWeek.Monday.ToString().ToUpper() }
                }
            })
            .Should()
            .BeEquivalentTo(new Container<DayOfWeek>(DayOfWeek.Monday));
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.StringCI)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek?>))]
public partial class NullableStringCIEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableStringCIEnumTests() : base(new DayOfWeek?[] { DayOfWeek.Monday, DayOfWeek.Friday, null },
        x => x is not null ? new AttributeValue { S = x.Value.ToString() } : null)
    {
    }

    protected override Container<DayOfWeek?> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    [Fact]
    public void Unmarshall_EnumIs_CaseInsensitive()
    {
        ContainerMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
            {
                {
                    nameof(Container<DayOfWeek>.Element),
                    new AttributeValue { S = DayOfWeek.Monday.ToString().ToUpper() }
                }
            })
            .Should()
            .BeEquivalentTo(new Container<DayOfWeek>(DayOfWeek.Monday));
    }
    
    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.Integer)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class IntEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public IntEnumTests() : base(Enum.GetValues<DayOfWeek>(), x => new AttributeValue { N = ((int)x).ToString() })
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

[DynamoDbMarshallerOptions(EnumConversionStrategy = EnumConversionStrategy.Integer)]
[DynamoDBMarshaller(typeof(Container<DayOfWeek?>))]
public partial class NullableIntEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableIntEnumTests() : base(Enum.GetValues<DayOfWeek>().Cast<DayOfWeek?>().Append(null),
        x => x is null ? null : new AttributeValue { N = ((int)x).ToString() })
    {
    }

    protected override Container<DayOfWeek?> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDBMarshaller(typeof(Container<DayOfWeek>))]
public partial class DefaultEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public DefaultEnumTests() : base(Enum.GetValues<DayOfWeek>(), x => new AttributeValue { N = ((int)x).ToString() })
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
public partial class DefaultNullableEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public DefaultNullableEnumTests() : base(Enum.GetValues<DayOfWeek>().Cast<DayOfWeek?>().Append(null),
        x => x is null ? null : new AttributeValue { N = ((int)x).ToString() })
    {
    }

    protected override Container<DayOfWeek?> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek?> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}