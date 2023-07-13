namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument;

[DynamoDbDocumentProperty(typeof(MainClass))]
public partial class AttributeExpressionTests
{
    [Fact]
    public void Value_References_AreDeterministic()
    {
        var references = new MainClass_Document.MainClassReferences(default, default);

        references.ThreeToSix.FiveToSix.Five.Value.Should().Be(":p5");
        references.SevenToEight.Seven.Value.Should().Be(":p7");

    }

    [Fact]
    public void Test()
    {
        MainClassDocument
            .ConditionExpression(x => $"{x.SevenToEight.Seven.Value} <> {x.SevenToEight.Seven.Name}").Expression
            .Should()
            .Be($":p7 <> #{nameof(MainClass.SevenToEight)}.#{nameof(MainClass.SubClassTwo.Seven)}");
    }

}

public class MainClass
{
    public string One { get; set; }
    public string Two { get; set; }
    public SubClassOne ThreeToSix { get; set; }
    public SubClassTwo SevenToEight { get; set; }

    public class SubClassOne
    {
        public string Three { get; set; }
        public string Four { get; set; }
        public SubSubClass FiveToSix { get; set; }

        public class SubSubClass
        {
            public string Five { get; set; }
            public string Six { get; set; }
        }

    }

    public class SubClassTwo
    {
        public string Seven { get; set; }
        public string Eight { get; set; }

    }

    public string Nine { get; set; }

}