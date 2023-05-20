using System;
using System.Collections.Generic;

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
                .WithMessage("The value is not supposed to be null, to allow this; make the property nullable. (Parameter 'EnabledNoneNullableReferenceType')");
        else
            result.Should().NotThrow();
    }

    [Theory]
    [InlineData("1", "2", "3")]
    [InlineData(null, "2", "3")]
    [InlineData("1", null, "3")]
    [InlineData(null, null, "3")]
    public void BuildAttributeValues_NestedGenericWith_NullabilityCheckNotThrows(
        string disabledNullableReferenceType,
        string enabledNullableReferenceType,
        string enabledNoneNullableReferenceType)
    {
        var @class = new NestedNullableAnnotationTestClass()
        {
            DisabledNullableReferenceType = new KeyValuePair<int, string>(1, disabledNullableReferenceType),
            EnabledNullableReferenceType = new KeyValuePair<int, string?>(1, enabledNullableReferenceType),
            EnabledNoneNullableReferenceType = new KeyValuePair<int, string>(1, enabledNoneNullableReferenceType)
        };

        var result = () => @class.BuildAttributeValues();

        result.Should().NotThrow();
    }

    [Theory]
    [InlineData("1", "2", null)]
    [InlineData(null, "2", null)]
    [InlineData(null, null, null)]
    public void BuildAttributeValues_NestedGenericWith_NullabilityCheckThrows(
        string disabledNullableReferenceType,
        string enabledNullableReferenceType,
        string enabledNoneNullableReferenceType)
    {
        var @class = new NestedNullableAnnotationTestClass()
        {
            DisabledNullableReferenceType = new KeyValuePair<int, string>(1, disabledNullableReferenceType),
            EnabledNullableReferenceType = new KeyValuePair<int, string?>(1, enabledNullableReferenceType),
            EnabledNoneNullableReferenceType = new KeyValuePair<int, string>(1, enabledNoneNullableReferenceType)
        };

        var result = () => @class.BuildAttributeValues();

        result.Should()
            .ThrowExactly<ArgumentNullException>();
    }
}

// TODO even though we do handle the nullablity outside the method invocation; we get warnings inside the method.
// To solve this we could get rid of the last nullablity of the parameter.  So we only generate signatures whose parameter is never nullable.
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
public partial class NestedNullableAnnotationTestClass
{
#nullable disable
    public KeyValuePair<int, string> DisabledNullableReferenceType { get; set; }
#nullable enable

    public KeyValuePair<int, string?> EnabledNullableReferenceType { get; set; }

    public KeyValuePair<int, string> EnabledNoneNullableReferenceType { get; set; }
}