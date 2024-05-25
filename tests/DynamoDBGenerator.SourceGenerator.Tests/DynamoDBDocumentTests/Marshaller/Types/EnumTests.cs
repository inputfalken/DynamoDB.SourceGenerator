using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Options;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Name)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Name)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.CaseInsensitiveName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
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
    public void Unmarshall_Parsing_ShouldBeCaseInsensitive()
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.CaseInsensitiveName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
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
    public void Unmarshall_Parsing_ShouldBeCaseInsensitive()
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Integer)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Integer)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
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

[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
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

[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.UpperCaseName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
public partial class UpperCaseEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public UpperCaseEnumTests() : base(Enum.GetValues<DayOfWeek>(), x =>  new AttributeValue { S = x.ToString().ToUpperInvariant() })
    {
    }

    protected override Container<DayOfWeek> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.UpperCaseName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
public partial class NullableUpperCaseEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableUpperCaseEnumTests() : base(Enum.GetValues<DayOfWeek>().Cast<DayOfWeek?>().Append(null),
        x => x is null ? null : new AttributeValue { S = x.Value.ToString().ToUpperInvariant() })
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

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.LowerCaseName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek>))]
public partial class LowerCaseEnumTests : RecordMarshalAsserter<DayOfWeek>
{
    public LowerCaseEnumTests() : base(Enum.GetValues<DayOfWeek>(), x =>  new AttributeValue { S = x.ToString().ToLowerInvariant() })
    {
    }

    protected override Container<DayOfWeek> UnmarshallImplementation(
        Dictionary<string, AttributeValue> attributeValues)
    {
        return ContainerMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(Container<DayOfWeek> element)
    {
        return ContainerMarshaller.Marshall(element);
    }
}

[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.LowerCaseName)]
[DynamoDBMarshaller(EntityType = typeof(Container<DayOfWeek?>))]
public partial class NullableLowerCaseEnumTests : RecordMarshalAsserter<DayOfWeek?>
{
    public NullableLowerCaseEnumTests() : base(Enum.GetValues<DayOfWeek>().Cast<DayOfWeek?>().Append(null),
        x => x is null ? null : new AttributeValue { S = x.Value.ToString().ToLowerInvariant() })
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
