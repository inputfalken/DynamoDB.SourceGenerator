using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(typeof(Cat))]
public partial class InheritanceTests
{
    [Fact]
    public void Includes_Inherited_DataMember()
    {
        var cat = new Cat {Name = "Mjaao", MjaoFactor = 100};

        CatMarshaller
            .Marshall(cat)
            .Should()
            .SatisfyRespectively(x =>
            {
                x.Key.Should().Be(nameof(cat.Name));
                x.Value.S.Should().Be("Mjaao");
            }, x =>
            {

                x.Key.Should().Be(nameof(cat.MjaoFactor));
                x.Value.N.Should().Be("100");
            });

    }


}

public class Animal
{
    public string Name { get; set; } = null!;

}

public class Cat : Animal
{
    public int MjaoFactor { get; set; }
}