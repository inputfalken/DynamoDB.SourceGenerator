namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument;

[DynamoDbDocumentProperty(typeof(MainClass))]
public partial class AttributeExpressionTests
{
    [Fact]
    public void Value_References_AreDeterministic()
    {
        var references = new MainClass_Document.MainClassReferences(default, default);

        references.Seven.Value.Should().Be(":p7");
        references.Seven.Name.Should().Be($"#{nameof(references.Seven)}");
        references.ClassTwo.Six.Value.Should().Be(":p6");
        references.ClassTwo.Six.Name.Should().Be($"#{nameof(references.ClassTwo)}.#{nameof(references.ClassTwo.Six)}");
        references.ClassTwo.Five.Value.Should().Be(":p5");
        references.ClassTwo.Five.Name.Should().Be($"#{nameof(references.ClassTwo)}.#{nameof(references.ClassTwo.Five)}");
    }

    [Fact]
    public void Test()
    {
        MainClassDocument
            .ConditionExpression(x => $"{x.ClassTwo.Five.Value} <> {x.ClassTwo.Five.Name}")
            .Expression
            .Should()
            .Be($":p5 <> #{nameof(MainClass.ClassTwo)}.#{nameof(MainClass.SubClassTwo.Five)}");
    }

}

public class MainClass
{
    public string One { get; set; }
    public string Two { get; set; }
    public SubClassOne ClassOne { get; set; }
    public SubClassTwo ClassTwo { get; set; }

    public class SubClassOne
    {
        public string Three { get; set; }
        public string Four { get; set; }

    }

    public class SubClassTwo
    {
        public string Five { get; set; }
        public string Six { get; set; }

    }

    public string Seven { get; set; }

}