namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class NullableTests
{
    [Fact]
    public void BuildAttributeValues_Default_IsSkipped()
    {
        var @class = new NullableValueTypeClass();

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void BuildAttributeValues_NonDefault_IsIncluded()
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

[DynamoDbDocument]
public partial class NullableValueTypeClass
{
    [DynamoDBProperty]
    public int? ValueType { get; set; }
}