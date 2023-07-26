namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBDocument(typeof(Person))]
[DynamoDBDocument(typeof(SelfReferencingClass))]
[DynamoDBDocument(typeof(ClassWithOverriddenAttributeName))]
public partial class ExpressionAttributeTrackerTests
{
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void SelfReference_AttributeReferences_EnsureUniqueness(int count)
    {
        var traversed = SelfReferencingClassDocument.TraverseBy(x => x.Self, count).ToArray();

        traversed
            .Select(x => x.Field1.Name)
            .Concat(traversed.Select(x => x.Field2.Name))
            .Distinct()
            .Should()
            .HaveCount(count * 2);

        traversed
            .Select(x => x.Field1.Value)
            .Concat(traversed.Select(x => x.Field2.Value))
            .Distinct()
            .Should()
            .HaveCount(count * 2);
    }

    [Fact]
    public void ClassWithOverriddenAttributeName_AttributeReferences_ShouldChangeNameValue()
    {
        var references = ClassWithOverriddenAttributeNameDocument.ExpressionAttributeTracker();

        references.Foo.Name.Should().Be("#SomethingElse");
        references.Foo.Value.Should().Be(":p1");

    }
    [Fact]
    public void Person_AttributeReferences_ShouldBeCorrectlySet()
    {
        var references = PersonDocument.ExpressionAttributeTracker();

        references.FirstName.Name.Should().Be("#FirstName");
        references.CreatedAt.Name.Should().Be("#CreatedAt");

        references.FirstName.Value.Should().Be(":p1");
        references.CreatedAt.Value.Should().Be(":p2");

        references.Address.Name.Value.Should().Be(":p3");
        references.Address.Name.Name.Should().Be("#Address.#Name");

        references.Address.Street.Name.Value.Should().Be(":p4");
        references.Address.Street.Name.Name.Should().Be("#Address.#Street.#Name");
    }

    [Fact]
    public void Person_AttributeReferences_ValueAreSetDynamicallySample1()
    {
        var references = PersonDocument.ExpressionAttributeTracker();

        references.Address.Street.Name.Value.Should().Be(":p1");
        references.FirstName.Value.Should().Be(":p2");
        references.CreatedAt.Value.Should().Be(":p3");
    }

    [Fact]
    public void Person_AttributeReferences_ValueAreSetDynamicallySample2()
    {
        var references = PersonDocument.ExpressionAttributeTracker();

        references.Address.Name.Value.Should().Be(":p1");
        references.CreatedAt.Value.Should().Be(":p2");
        references.FirstName.Value.Should().Be(":p3");
    }
}

public static class AssertionExtensions
{

    public static IEnumerable<T2> TraverseBy<T, T2>(this IDynamoDBDocument<T, T2> source, Func<T2, T2> recursiveSelector, int count) where T2 : IExpressionAttributeReferences<T>
    {
        var attributeReferences = source.ExpressionAttributeTracker();

        for (var i = 0; i < count; i++)
        {
            yield return attributeReferences;

            attributeReferences = recursiveSelector(attributeReferences);
        }
    }
}

public class ClassWithOverriddenAttributeName
{
    [DynamoDBProperty("SomethingElse")]
    public string Foo { get; set; } = null!;
}

public class SelfReferencingClass
{
    public string Field1 { get; set; } = null!;
    public SelfReferencingClass Self { get; set; } = null!;
    public string Field2 { get; set; } = null!;
}

public class Person
{
    public string FirstName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public AddressModel Address { get; set; } = null!;

    public class AddressModel
    {
        public string Name { get; set; } = null!;
        public StreetModel Street { get; set; } = null!;

        public class StreetModel
        {
            public string Name { get; set; } = null!;
        }
    }
}