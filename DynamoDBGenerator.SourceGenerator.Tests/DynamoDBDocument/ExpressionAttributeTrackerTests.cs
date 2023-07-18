namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocument;

/// <summary>
/// NOTE the order of the assertions are intended to prove that the order you access the properties should not change their underlying value.
/// </summary>
[DynamoDbDocumentProperty(typeof(TripleNestedClass))]
[DynamoDbDocumentProperty(typeof(FlatClass))]
[DynamoDbDocumentProperty(typeof(SelfReferencingClass))]
[DynamoDbDocumentProperty(typeof(NestedClass))]
public partial class ExpressionAttributeTrackerTests
{
    [Fact]
    public void TripleNestedClass_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = TripleNestedClassDocument.ExpressionAttributeTracker();

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
    public void NestedClass_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = NestedClassDocument.ExpressionAttributeTracker();

        references.One.Value.Should().Be(":p1");
        references.One.Name.Should().Be("#One");
        references.Three.Value.Should().Be(":p3");
        references.Three.Name.Should().Be("#Three");
        references.Nested.Two.Value.Should().Be(":p2");
        references.Nested.Two.Name.Should().Be("#Nested.#Two");
    }

    [Fact]
    public void FlatClass_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = FlatClassDocument.ExpressionAttributeTracker();

        references.Two.Value.Should().Be(":p2");
        references.Two.Name.Should().Be("#Two");

        references.One.Value.Should().Be(":p1");
        references.One.Name.Should().Be("#One");

        references.Three.Value.Should().Be(":p3");
        references.Three.Name.Should().Be("#Three");
    }

    [Fact]
    public void SelfReference_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = SelfReferencingClassDocument.ExpressionAttributeTracker();
        references.PlusOne.Value.Should().Be(":p1");
        references.PlusOne.Name.Should().Be("#PlusOne");

        // One would expect that this should be 3
        references.PlusAnotherOne.Value.Should().Be(":p4");
        references.PlusAnotherOne.Name.Should().Be("#PlusAnotherOne");

        references.Self.PlusOne.Value.Should().Be(":p2");
        references.Self.PlusOne.Name.Should().Be("#Self.#PlusOne");

        // One would expect that this should be 4
        references.Self.PlusAnotherOne.Value.Should().Be(":p5");
        references.Self.PlusAnotherOne.Name.Should().Be("#Self.#PlusAnotherOne");

    }

    [Fact]
    public void SelfReference_AttributeReferences_EnsureUniqueness()
    {
        const int count = 5;
        var traversed = SelfReferencingClassDocument.TraverseBy(x => x.Self, count).ToArray();

        traversed
            .Select(x => x.PlusOne.Name)
            .Concat(traversed.Select(x => x.PlusAnotherOne.Name))
            .Distinct()
            .Should()
            .HaveCount(count * 2);

        traversed
            .Select(x => x.PlusOne.Value)
            .Concat(traversed.Select(x => x.PlusAnotherOne.Value))
            .Distinct()
            .Should()
            .HaveCount(count * 2);
    }

}

public static class AssertionExtensions
{

    public static IEnumerable<T2> TraverseBy<T, T2>(this IDynamoDbDocument<T, T2> source, Func<T2, T2> recursiveSelector, int count) where T2 : IExpressionAttributeReferences<T>
    {
        var attributeReferences = source.ExpressionAttributeTracker();

        for (var i = 0; i < count; i++)
        {
            yield return attributeReferences;

            attributeReferences = recursiveSelector(attributeReferences);
        }
    }
}

public class NestedClass
{
    public string One { get; set; }
    public NestedClass2 Nested { get; set; }
    public string Three { get; set; }

    public class NestedClass2
    {
        public string Two { get; set; }
    }
}

public class SelfReferencingClass
{
    public string PlusOne { get; set; }
    public SelfReferencingClass Self { get; set; }
    public string PlusAnotherOne { get; set; }
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