using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(typeof(ValueTypeClass))]
public partial class ValueTypeTests
{
    [Fact]
    public void Serialize_ValueType_DefaultValueIsIncluded()
    {
        var @class = new ValueTypeClass();

        ValueTypeClassMarshaller
            .Marshall(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(ValueTypeClass.ValueType))
            .And
            .ContainSingle(x => x.Value.N == default(int).ToString());
    }
}

public class ValueTypeClass
{
    [DynamoDBProperty]
    public int ValueType { get; set; }
}
