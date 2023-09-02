using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBMarshaller(typeof(PropertyWithMixedNames))]
public partial class PropertyRenamingTests
{
    [Fact]
    public void Serialize_Attributes_ChangesNames()
    {

        var propertyAssertion = PropertyWithMixedNamesMarshaller
            .Deserialize(new Dictionary<string, AttributeValue>
            {
                {nameof(PropertyWithMixedNames.PlainProperty), new AttributeValue {S = "1"}},
                {nameof(PropertyWithMixedNames.PropertyWithDdbPropertyAttribute), new AttributeValue {S = "2"}},
                {"AnotherName", new AttributeValue {S = "3"}},
                {nameof(PropertyWithMixedNames.PropertyWithDdbHashKeyAttribute), new AttributeValue {S = "4"}},
                {"AnotherHashKey", new AttributeValue {S = "5"}},
                {nameof(PropertyWithMixedNames.PropertyWithDdbRangeKeyAttribute), new AttributeValue {S = "6"}},
                {"AnotherRangeKey", new AttributeValue {S = "7"}}
            })
            .Should()
            .BeOfType<PropertyWithMixedNames>();

        propertyAssertion.Which.PlainProperty.Should().Be("1");
        propertyAssertion.Which.PropertyWithDdbPropertyAttribute.Should().Be("2");
        propertyAssertion.Which.PropertyWithDdbPropertyAttributeWithOverridenName.Should().Be("3");
        propertyAssertion.Which.PropertyWithDdbHashKeyAttribute.Should().Be("4");
        propertyAssertion.Which.PropertyWithDdbHashKeyAttributeWithOverridenName.Should().Be("5");
        propertyAssertion.Which.PropertyWithDdbRangeKeyAttribute.Should().Be("6");
        propertyAssertion.Which.PropertyWithDdbRangeKeyAttributeWithOverridenName.Should().Be("7");
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