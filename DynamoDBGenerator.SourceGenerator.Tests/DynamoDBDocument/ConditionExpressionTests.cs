namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument;

/// <summary>
/// NOTE the order of the assertions are intended to prove that the order you access the properties should not change their underlying value.
/// </summary>
[DynamoDbDocumentProperty(typeof(TripleNestedClass))]
[DynamoDbDocumentProperty(typeof(FlatClass))]
public partial class AttributeExpressionTests
{
    [Fact]
    public void TripleNestedClass_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = new TripleNestedClass_Document.TripleNestedClassReferences(null, 0);

        references.ThreeToSix.FiveToSix.Five.Value.Should().Be(":p5");
        references.ThreeToSix.FiveToSix.Five.Name.Should().Be("#ThreeToSix.#FiveToSix.#Five");
        
        references.SevenToEight.Seven.Value.Should().Be(":p7");
        references.SevenToEight.Seven.Name.Should().Be("#SevenToEight.#Seven");
        
        references.Nine.Value.Should().Be(":p9");
        references.Nine.Name.Should().Be("#Nine");
        
        references.Two.Value.Should().Be(":p2");
        references.Two.Name.Should().Be("#Two");
        
        references.One.Value.Should().Be(":p1");
        references.One.Name.Should().Be("#One");
        
        references.ThreeToSix.Three.Value.Should().Be(":p3");
        references.ThreeToSix.Three.Name.Should().Be("#ThreeToSix.#Three");

    }
    
    [Fact]
    public void FlatClass_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = new FlatClass_Document.FlatClassReferences(null, 0);

        references.Two.Value.Should().Be(":p2");
        references.Two.Name.Should().Be("#Two");
        
        references.One.Value.Should().Be(":p1");
        references.One.Name.Should().Be("#One");
        
        references.Three.Value.Should().Be(":p3");
        references.Three.Name.Should().Be("#Three");

    }
}

public class FlatClass
{
    public string One { get; set; }
    public string Two { get; set; }
    public string Three { get; set; }
}

public class TripleNestedClass
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