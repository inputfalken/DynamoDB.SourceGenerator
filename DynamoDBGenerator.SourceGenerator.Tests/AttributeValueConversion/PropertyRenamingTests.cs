namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion;

public class PropertyRenamingTests
{
    [Fact]
    public void BuildAttributeValues_Attributes_ChangesNames()
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

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(
            x => { x.Key.Should().Be(nameof(PropertyWithMixedNames.PlainProperty)); },
            x => { x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbPropertyAttribute)); },
            x => { x.Key.Should().Be("AnotherName"); },
            x => { x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbHashKeyAttribute)); },
            x => { x.Key.Should().Be("AnotherHashKey"); },
            x => { x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbRangeKeyAttribute)); },
            x => { x.Key.Should().Be("AnotherRangeKey"); }
        );
    }
}

[DynamoDbDocument]
public partial class PropertyWithMixedNames
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