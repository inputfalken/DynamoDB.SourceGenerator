namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class ValueTypeTests
{
    [Fact]
    public void BuildAttributeValues_ValueType_DefaultValueIsIncluded()
    {
        var @class = new ValueTypeClass();

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ValueTypeClass.ValueType))
            .And
            .ContainSingle(x => x.Value.N == default(int).ToString());
    }

    [Fact]
    public void BuildAttributeValues_NullableValueType_DefaultValueIsSkipped()
    {
        var @class = new NullableValueTypeClass();

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }
    
    [Fact]
    public void BuildAttributeValues_NullableValueType_IsIncluded()
    {
        var @class = new NullableValueTypeClass()
        {
            ValueType = 1
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(NullableValueTypeClass.ValueType))
            .And
            .ContainSingle(x => x.Value.N == @class.ValueType.ToString());
    }
}

[AttributeValueGenerator]
public partial class NullableValueTypeClass
{
    [DynamoDBProperty]
    public int? ValueType { get; set; }
}

[AttributeValueGenerator]
public partial class ValueTypeClass
{
    [DynamoDBProperty]
    public int ValueType { get; set; }
}