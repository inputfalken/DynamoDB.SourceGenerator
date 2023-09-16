using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof(Person))]
[DynamoDBMarshaller(typeof(SelfReferencingClass))]
[DynamoDBMarshaller(typeof(ClassWithOverriddenAttributeName))]
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
        var traversedNameTracker = SelfReferencingClassMarshaller.TraverseByNameTracker(x => x.Self, count).ToArray();
        traversedNameTracker
            .Select(x => x.Field1)
            .Concat(traversedNameTracker.Select(x => x.Field2))
            .Distinct()
            .Should()
            .HaveCount(count * 2);

        var traversedValueTracker = SelfReferencingClassMarshaller.TraverseByValueTracker(x => x.Self, count).ToArray();
        traversedValueTracker
            .Select(x => x.Field1)
            .Concat(traversedNameTracker.Select(x => x.Field2))
            .Distinct()
            .Should()
            .HaveCount(count * 2);
    }

    [Fact]
    public void ClassWithOverriddenAttributeName_AttributeReferences_ShouldChangeNameValue()
    {
        var nameTracker = ClassWithOverriddenAttributeNameMarshaller.AttributeNameExpressionTracker();
        var valueTracker = ClassWithOverriddenAttributeNameMarshaller.AttributeExpressionValueTracker();

        nameTracker.Foo.Should().Be("#SomethingElse");
        valueTracker.Foo.Should().Be(":p1");

    }
    [Fact]
    public void Person_AttributeReferences_ShouldBeCorrectlySet()
    {
        var nameTracker = PersonMarshaller.AttributeNameExpressionTracker();
        var valueTracker = PersonMarshaller.AttributeExpressionValueTracker();

        nameTracker.FirstName.Should().Be("#FirstName");
        nameTracker.CreatedAt.Should().Be("#CreatedAt");

        valueTracker.FirstName.Should().Be(":p1");
        valueTracker.CreatedAt.Should().Be(":p2");

        valueTracker.Address.Name.Should().Be(":p3");
        nameTracker.Address.Name.Should().Be("#Address.#Name");

        valueTracker.Address.Street.Name.Should().Be(":p4");
        nameTracker.Address.Street.Name.Should().Be("#Address.#Street.#Name");
    }

    [Fact]
    public void Person_AttributeReferences_ValueAreSetDynamicallySample1()
    {
        var valueTracker = PersonMarshaller.AttributeExpressionValueTracker();

        valueTracker.Address.Street.Name.Should().Be(":p1");
        valueTracker.FirstName.Should().Be(":p2");
        valueTracker.CreatedAt.Should().Be(":p3");
    }

    [Fact]
    public void Person_AttributeReferences_ValueAreSetDynamicallySample2()
    {
        var valueTracker = PersonMarshaller.AttributeExpressionValueTracker();

        valueTracker.Address.Name.Should().Be(":p1");
        valueTracker.CreatedAt.Should().Be(":p2");
        valueTracker.FirstName.Should().Be(":p3");
    }
}

public static class AssertionExtensions
{

    public static IEnumerable<T4> TraverseByValueTracker<T, T2, T3, T4>(this IDynamoDBMarshaller<T, T2, T3, T4> source, Func<T4, T4> recursiveSelector, int count)
        where T3 : IExpressionAttributeNameTracker
        where T4 : IExpressionAttributeValueTracker<T2>
    {
        var attributeReferences = source.AttributeExpressionValueTracker();

        for (var i = 0; i < count; i++)
        {
            yield return attributeReferences;

            attributeReferences = recursiveSelector(attributeReferences);
        }
    }
    public static IEnumerable<T3> TraverseByNameTracker<T, T2, T3, T4>(
        this IDynamoDBMarshaller<T, T2, T3, T4> source,
        Func<T3, T3> recursiveSelector,
        int count
    )
        where T3 : IExpressionAttributeNameTracker
        where T4 : IExpressionAttributeValueTracker<T2>
    {
        var attributeReferences = source.AttributeNameExpressionTracker();

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