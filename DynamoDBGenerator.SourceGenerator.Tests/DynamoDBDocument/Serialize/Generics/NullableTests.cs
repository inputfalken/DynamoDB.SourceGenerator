namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Generics;

[DynamoDBGenerator.DynamoDBDocument(typeof(NullableValueTypeClass))]
public partial class NullableTests
{
    [Fact]
    public void Serialize_Default_IsSkipped()
    {
        var @class = new NullableValueTypeClass();

        NullableValueTypeClassDocument
            .Serialize(@class)
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

        NullableValueTypeClassDocument
            .Serialize(@class)
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
