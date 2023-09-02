namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(typeof(PropertyWithMixedNames))]
public partial class PropertyRenamingTests
{
    [Fact]
    public void Serialize_Attributes_ChangesNames()
    {
        var @class = new PropertyWithMixedNames
        {
            PlainProperty = "1",
            PropertyWithDdbPropertyAttribute = "2",
            PropertyWithDdbPropertyAttributeWithOverridenName = "3",
            PropertyWithDdbHashKeyAttribute = "4",
            PropertyWithDdbHashKeyAttributeWithOverridenName = "5",
            PropertyWithDdbRangeKeyAttribute = "6",
            PropertyWithDdbRangeKeyAttributeWithOverridenName = "7"
        };

        PropertyWithMixedNamesMarshaller
            .Marshall(@class)
            .Should()
            .SatisfyRespectively(
                x =>
                {
                    x.Key.Should().Be(nameof(PropertyWithMixedNames.PlainProperty));
                    x.Value.S.Should().Be("1");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbPropertyAttribute));
                    x.Value.S.Should().Be("2");
                },
                x =>
                {
                    x.Key.Should().Be("AnotherName");
                    x.Value.S.Should().Be("3");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbHashKeyAttribute));
                    x.Value.S.Should().Be("4");
                },
                x =>
                {
                    x.Key.Should().Be("AnotherHashKey");
                    x.Value.S.Should().Be("5");
                },
                x =>
                {
                    x.Key.Should().Be(nameof(PropertyWithMixedNames.PropertyWithDdbRangeKeyAttribute));
                    x.Value.S.Should().Be("6");
                },
                x =>
                {
                    x.Key.Should().Be("AnotherRangeKey");
                    x.Value.S.Should().Be("7");
                }
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