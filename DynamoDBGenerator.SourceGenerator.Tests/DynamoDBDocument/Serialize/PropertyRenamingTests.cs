namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize;

[DynamoDBGenerator.DynamoDBDocument(typeof(PropertyWithMixedNames))]
public partial class PropertyRenamingTests
{
    [Fact]
    public void Serialize_Attributes_ChangesNames()
    {
        var @class = new PropertyWithMixedNames
        {
            PlainProperty = "N/A",
            PropertyWithDdbPropertyAttribute = "N/A",
            PropertyWithDdbPropertyAttributeWithOverridenName = "N/A",
            PropertyWithDdbHashKeyAttribute = "N/A",
            PropertyWithDdbHashKeyAttributeWithOverridenName = "N/A",
            PropertyWithDdbRangeKeyAttribute = "N/A",
            PropertyWithDdbRangeKeyAttributeWithOverridenName = "N/A"
        };

        PropertyWithMixedNamesDocument
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(
                x => { ((string)x.Key).Should().Be(nameof(PropertyWithMixedNames.PlainProperty)); },
                x => { ((string)x.Key).Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbPropertyAttribute)); },
                x => { ((string)x.Key).Should().Be("AnotherName"); },
                x => { ((string)x.Key).Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbHashKeyAttribute)); },
                x => { ((string)x.Key).Should().Be("AnotherHashKey"); },
                x => { ((string)x.Key).Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbRangeKeyAttribute)); },
                x => { ((string)x.Key).Should().Be("AnotherRangeKey"); }
            );
    }
}

public class PropertyWithMixedNames
{
    public string PlainProperty { get; init; } = null!;

    [DynamoDBProperty]
    public string PropertyWithDdbPropertyAttribute { get; init; } = null!;

    [DynamoDBProperty("AnotherName")]
    public string PropertyWithDdbPropertyAttributeWithOverridenName { get; init; } = null!;

    [DynamoDBHashKey]
    public string PropertyWithDdbHashKeyAttribute { get; init; } = null!;

    [DynamoDBHashKey("AnotherHashKey")]
    public string PropertyWithDdbHashKeyAttributeWithOverridenName { get; init; } = null!;

    [DynamoDBRangeKey]
    public string PropertyWithDdbRangeKeyAttribute { get; init; } = null!;

    [DynamoDBRangeKey("AnotherRangeKey")]
    public string PropertyWithDdbRangeKeyAttributeWithOverridenName { get; init; } = null!;
}
