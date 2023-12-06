using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(Character))]
public partial class CharTests
{
    private static readonly Character Dto = new('A');

    private static readonly Dictionary<string, AttributeValue> AttributeValues = new()
    {
        {nameof(Character.Letter), new AttributeValue {S = "A"}}
    };

    [Fact(Skip = "TODO")]
    public void Unmarshall_EmptyString_ShouldThrow()
    {
        var act = () => CharacterMarshaller.Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Character.Letter), new AttributeValue {S = ""}}});
        act.Should().Throw<DynamoDBMarshallingException>();
    }

    [Fact]
    public void Marshall()
    {
        CharacterMarshaller.Marshall(Dto).Should().BeEquivalentTo(AttributeValues);
    }

    [Fact]
    public void Unmarshall()
    {
        CharacterMarshaller.Unmarshall(AttributeValues).Should().BeEquivalentTo(Dto);
    }

    public record Character(char Letter);
}