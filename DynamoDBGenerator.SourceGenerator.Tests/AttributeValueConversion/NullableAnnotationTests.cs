namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class NullableAnnotationTests
{
    [Theory]
    [InlineData(1, "1", "2", "3", false)]
    [InlineData(null, "1", "2", "3", false)]
    [InlineData(1, null, "2", "3", false)]
    [InlineData(1, "1", null, "3", false)]
    [InlineData(1, null, null, "3", false)]
    [InlineData(1, "1", "2", null, true)]
    [InlineData(1, null, "2", null, true)]
    [InlineData(1, null, null, null, true)]
    public void BuildAttributeValues_AllFieldsSet_ShouldNotThrow(
        int? nullableValueType,
        string disabledNullableReferenceType,
        string enabledNullableReferenceType,
        string enabledNoneNullableReferenceType,
        bool shouldThrow)
    {
        var @class = new NullableAnnotationTestClass
        {
            NullableValueType = nullableValueType,
            DisabledNullableReferenceType = disabledNullableReferenceType,
            EnabledNullableReferenceType = enabledNullableReferenceType,
            EnabledNoneNullableReferenceType = enabledNoneNullableReferenceType
        };

        var result = () => @class.BuildAttributeValues();

        if (shouldThrow)
            result.Should()
                .ThrowExactly<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'EnabledNoneNullableReferenceType')");
        else
            result.Should().NotThrow();
    }
}


[AttributeValueGenerator]
public partial class NullableAnnotationTestClass
{
#nullable disable
    public string DisabledNullableReferenceType { get; set; }
#nullable enable

    public string? EnabledNullableReferenceType { get; set; }

    public string EnabledNoneNullableReferenceType { get; set; } = null!;

    public int? NullableValueType { get; set; }

}