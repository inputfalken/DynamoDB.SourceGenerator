using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize;

[DynamoDBMarshaller(typeof(Cat))]
public partial class InheritanceTests
{
    [Fact]
    public void Includes_Inherited_DataMember()
    {

        var result = CatMarshaller.Unmarshall(new Dictionary<string, AttributeValue>
        {
            {nameof(Cat.MjaoFactor), new AttributeValue {N = "100"}},
            {nameof(Cat.Name), new AttributeValue {S = "Mjaao"}}
        });

        result.MjaoFactor.Should().Be(100);
        result.Name.Should().Be("Mjaao");
    }

}

public class Animal
{
    public string Name { get; set; }

}

public class Cat : Animal
{
    public int MjaoFactor { get; set; }
}