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
}

[DynamoDbDocument]
public partial class ValueTypeClass
{
    [DynamoDBProperty]
    public int ValueType { get; set; }
}