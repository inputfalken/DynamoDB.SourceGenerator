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
    public void BuildAttributeValues_With_NullabilityChecks(
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

    // The reason these tests are failing is due to that we're only supporting one signature per type signature.
    // Since nullable reference types are not a real type we wont be able to create separate signatures due to collisions with the method signature.
    [Theory]
    [InlineData("1", "2", "3", false)]
    [InlineData(null, "2", "3", false)]
    [InlineData("1", null, "3", false)]
    [InlineData(null, null, "3", false)]
    [InlineData("1", "2", null, true)]
    [InlineData(null, "2", null, true)]
    [InlineData(null, null, null, true)]
    public void BuildAttributeValues_NestedGenericWith_NullabilityChecks(
        string disabledNullableReferenceType,
        string enabledNullableReferenceType,
        string enabledNoneNullableReferenceType,
        bool shouldThrow)
    {
        var @class = new NesteNullableAnnotationTestclass()
        {
            DisabledNullableReferenceType = new KeyValuePair<string, int>(disabledNullableReferenceType, 1),
            EnabledNullableReferenceType = new KeyValuePair<string?, int>(enabledNullableReferenceType, 1),
            EnabledNoneNullableReferenceType = new KeyValuePair<string, int>(enabledNoneNullableReferenceType, 1)
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

[AttributeValueGenerator]
public partial class NesteNullableAnnotationTestclass
{
#nullable disable
    public KeyValuePair<string, int> DisabledNullableReferenceType { get; set; }
#nullable enable

    public KeyValuePair<string?, int> EnabledNullableReferenceType { get; set; }

    public KeyValuePair<string, int> EnabledNoneNullableReferenceType { get; set; }
}