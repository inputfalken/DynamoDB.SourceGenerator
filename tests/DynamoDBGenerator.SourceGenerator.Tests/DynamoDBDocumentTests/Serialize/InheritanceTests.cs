using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(EntityType = typeof(Cat))]
public partial class InheritanceTests
{
    [Fact]
    public void Includes_Inherited_DataMember()
    {
        var cat = new Cat { Name = "Mjaao", MjaoFactor = 100 };

        CatMarshaller
            .Marshall(cat)
            .Should()
            .BeEquivalentTo(new Dictionary<string, AttributeValue>
            {
                { nameof(Cat.MjaoFactor), new AttributeValue { N = "100" } },
                { nameof(Cat.Name), new AttributeValue { S = "Mjaao" } }
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