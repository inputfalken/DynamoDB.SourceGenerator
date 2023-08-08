using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBDocument(typeof(CharClass))]
public partial class CharTests
{
    [Fact]
    public void Deserialize_CharProperty_IsIncluded()
    {
        CharClassDocument
            .Deserialize(new Dictionary<string, AttributeValue> {{nameof(CharClass.Letter), new AttributeValue {S = "A"}}})
            .Should()
            .BeOfType<CharClass>()
            .Which
            .Letter
            .Should()
            .Be('A');
    }

    [Fact(Skip = "TODO Add check to verify that the string is only of 1 character.")]
    public void Deserialize_StringProperty_ShouldThrow()
    {
        var act = () => CharClassDocument
            .Deserialize(new Dictionary<string, AttributeValue> {{nameof(CharClass.Letter), new AttributeValue {S = "AAA"}}});

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class CharClass
{
    public char Letter { get; set; }
}