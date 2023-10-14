using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Generics;

[DynamoDBMarshaller(typeof(NullableValueTypeClass))]
public partial class NullableTests
{
    [Fact]
    public void Serialize_Default_IsSkipped()
    {
        var @class = new NullableValueTypeClass();

        NullableValueTypeClassMarshaller
            .Marshall(@class)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Serialize_NonDefault_IsIncluded()
    {
        var @class = new NullableValueTypeClass
        {
            ValueType = 1
        };

        NullableValueTypeClassMarshaller
            .Marshall(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(NullableValueTypeClass.ValueType))
            .And
            .ContainSingle(x => x.Value.N == @class.ValueType.ToString());
    }
}

public class NullableValueTypeClass
{
    [DynamoDBProperty]
    public int? ValueType { get; set; }
}
