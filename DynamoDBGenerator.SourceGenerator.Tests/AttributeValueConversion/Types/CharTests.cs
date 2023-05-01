namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class CharTests
{
    [Fact]
    public void BuildAttributeValues_CharProperty_IsIncluded()
    {
        var @class = new CharClass
        {
            Letter = 'L'
        };

        var result = @class.BuildAttributeValues();

        result.Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(nameof(CharClass.Letter));

            x.Value.S.Should().Be("L");

        });
    }
}

[AttributeValueGenerator]
public partial class CharClass
{
    public char Letter { get; set; }
}