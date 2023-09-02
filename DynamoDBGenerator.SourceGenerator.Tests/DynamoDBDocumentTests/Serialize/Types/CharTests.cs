namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBMarshaller(typeof(CharClass))]
public partial class CharTests
{
    [Fact]
    public void Serialize_CharProperty_IsIncluded()
    {
        var @class = new CharClass
        {
            Letter = 'L'
        };

        CharClassMarshaller
            .Serialize(@class)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(CharClass.Letter));

                x.Value.S.Should().Be("L");

            });
    }
}

public class CharClass
{
    public char Letter { get; set; }
}
