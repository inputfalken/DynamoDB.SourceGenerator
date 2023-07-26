namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument.Serialize.Types;

[DynamoDBGenerator.DynamoDBDocument(typeof(CharClass))]
public partial class CharTests
{
    [Fact]
    public void Serialize_CharProperty_IsIncluded()
    {
        var @class = new CharClass
        {
            Letter = 'L'
        };

        CharClassDocument
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(x =>
            {
                ((string)x.Key).Should().Be(nameof(CharClass.Letter));

                ((string)x.Value.S).Should().Be("L");

            });
    }
}

public class CharClass
{
    public char Letter { get; set; }
}
