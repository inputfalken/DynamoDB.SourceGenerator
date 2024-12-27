using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(EntityType = typeof(NestedNullableAnnotationTestClass))]
[DynamoDBMarshaller(EntityType = typeof(NullableAnnotationTestClass))]
public partial class NullableAnnotationTests
{
    [Theory]
    [InlineData("1", "2", "3")]
    [InlineData(null, "2", "3")]
    [InlineData("1", null, "3")]
    [InlineData(null, null, "3")]
    public void Serialize_NestedGenericWith_NullabilityCheckNotThrows(
        string? disabledNullableReferenceType,
        string? enabledNullableReferenceType,
        string enabledNoneNullableReferenceType)
    {
        var @class = new NestedNullableAnnotationTestClass
        {
            DisabledNullableReferenceType = (1, disabledNullableReferenceType),
            EnabledNullableReferenceType = (1, enabledNullableReferenceType),
            EnabledNoneNullableReferenceType = (1, enabledNoneNullableReferenceType)
        };

        var result = () => NestedNullableAnnotationTestClassMarshaller.Marshall(@class);

        result.Should().NotThrow();
    }

    [Theory]
    [InlineData("1", "2", null)]
    [InlineData(null, "2", null)]
    [InlineData(null, null, null)]
    public void Serialize_NestedGenericWith_NullabilityCheckThrows(
        string? disabledNullableReferenceType,
        string? enabledNullableReferenceType,
        string? enabledNoneNullableReferenceType)
    {
        var @class = new NestedNullableAnnotationTestClass
        {
            DisabledNullableReferenceType = (1, disabledNullableReferenceType),
            EnabledNullableReferenceType = (1, enabledNullableReferenceType),
            EnabledNoneNullableReferenceType = (1, enabledNoneNullableReferenceType!)
        };

        var result = () => NestedNullableAnnotationTestClassMarshaller.Marshall(@class);

        result.Should()
            .ThrowExactly<DynamoDBMarshallingException>()
            .WithMessage(
                "The data member is not supposed to be null, to allow this; make the data member nullable. (Data member 'EnabledNoneNullableValue')");
    }

    [Theory]
    [InlineData(1, "1", "2", "3", false)]
    [InlineData(null, "1", "2", "3", false)]
    [InlineData(1, null, "2", "3", false)]
    [InlineData(1, "1", null, "3", false)]
    [InlineData(1, null, null, "3", false)]
    [InlineData(1, "1", "2", null, true)]
    [InlineData(1, null, "2", null, true)]
    [InlineData(1, null, null, null, true)]
    public void Serialize_With_NullabilityChecks(
        int? nullableValueType,
        string? disabledNullableReferenceType,
        string? enabledNullableReferenceType,
        string? enabledNoneNullableReferenceType,
        bool shouldThrow)
    {
        var @class = new NullableAnnotationTestClass
        {
            NullableValueType = nullableValueType,
            DisabledNullableReferenceType = disabledNullableReferenceType,
            EnabledNullableReferenceType = enabledNullableReferenceType,
            EnabledNoneNullableReferenceType = enabledNoneNullableReferenceType!
        };

        var result = () => NullableAnnotationTestClassMarshaller.Marshall(@class);

        if (shouldThrow)
            result.Should()
                .ThrowExactly<DynamoDBMarshallingException>()
                .WithMessage(
                    "The data member is not supposed to be null, to allow this; make the data member nullable. (Data member 'EnabledNoneNullableReferenceType')");
        else
            result.Should().NotThrow();
    }
}

public class NullableAnnotationTestClass
{
#nullable disable
    public string DisabledNullableReferenceType { get; set; }
#nullable enable

    public string? EnabledNullableReferenceType { get; set; }

    public string EnabledNoneNullableReferenceType { get; set; } = null!;

    public int? NullableValueType { get; set; }
}

public class NestedNullableAnnotationTestClass
{
#nullable disable
    public (int DisabledNullableKey, string DisabledNullableValue) DisabledNullableReferenceType { get; set; }
#nullable enable

    public (int EnabledNullableKey, string? EnabledNullableValue) EnabledNullableReferenceType { get; set; }

    public (int EnabledNullableKey, string EnabledNoneNullableValue) EnabledNoneNullableReferenceType { get; set; }
}