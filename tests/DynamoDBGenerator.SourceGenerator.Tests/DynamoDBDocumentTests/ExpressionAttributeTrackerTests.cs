using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(EntityType = typeof(Person), ArgumentType = typeof((string firstName, DateTime timeStamp)),
    AccessName = "PersonWithTupleArgument")]
[DynamoDBMarshaller(EntityType = typeof(Person))]
[DynamoDBMarshaller(EntityType = typeof(SelfReferencingClass))]
[DynamoDBMarshaller(EntityType = typeof(ClassWithOverriddenAttributeName))]
[DynamoDBMarshaller(EntityType = typeof(InheritedClass))]
[DynamoDBMarshaller(EntityType = typeof(DuplicatedNavigationProperties))]
public partial class ExpressionAttributeTrackerTests
{
    [Fact]
    public void PersonWithTupleArgument_AccessingRootExpressionAttributeName_ShouldThrow()
    {
        var nameTracker = PersonWithTupleArgument.AttributeExpressionNameTracker();

        var act = () => nameTracker.ToString();

        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void PersonWithTupleArgument_AccessingRootExpressionAttributeValue_ShouldNotThrow()
    {
        var valueTracker = PersonWithTupleArgument.AttributeExpressionValueTracker();
        var tracker = valueTracker as IAttributeExpressionValueTracker<(string firstName, DateTime timeStamp)>;

        var act = () => valueTracker.ToString();
        act.Should().NotThrow();
        tracker.ToString().Should().Be(":p1");
    }

    [Fact]
    public void PersonWithTupleArgument_AccessingNestedExpressionAttributeName_ShouldNotThrow()
    {
        var nameTracker = PersonWithTupleArgument.AttributeExpressionNameTracker();

        var act = () => nameTracker.Address.ToString();

        act.Should().NotThrow();
        nameTracker.Address.ToString().Should().Be("#Address");
    }

    [Fact]
    public void PersonWithTupleArgument_Tuple_CanBeParameterized()
    {
        var valueTracker = PersonWithTupleArgument.AttributeExpressionValueTracker();
        var nameTracker = PersonWithTupleArgument.AttributeExpressionNameTracker();

        valueTracker.firstName.Should().Be(":p1");
        valueTracker.timeStamp.Should().Be(":p2");
        nameTracker.FirstName.Should().Be("#FirstName");
        nameTracker.CreatedAt.Should().Be("#CreatedAt");

        IAttributeExpressionValueTracker<(string firstName, DateTime timeStamp)> convertedValueTracker = valueTracker;

        var timeStamp = DateTime.Now;
        convertedValueTracker.AccessedValues(("Rob", timeStamp)).Should().SatisfyRespectively(x =>
        {
            x.Key.Should().Be(":p1");
            x.Value.S.Should().Be("Rob");
        }, x =>
        {
            x.Key.Should().Be(":p2");
            x.Value.S.Should().Be(timeStamp.ToString("O"));
        });
    }


    [Fact]
    public void SelfReference_AttributeNames_EnsureUniquness()
    {
        var nametracker = SelfReferencingClassMarshaller.AttributeExpressionNameTracker();
        var field1 = nametracker.Self.Self.Self.Self.Field1;
        var field2 = nametracker.Self.Self.Self.Field2;

        (nametracker as IAttributeExpressionNameTracker)
            .AccessedNames()
            .Should()
            .BeEquivalentTo(new[]
            {
                new KeyValuePair<string, string>("#Self", "Self"),
                new KeyValuePair<string, string>("#Field1", "Field1"),
                new KeyValuePair<string, string>("#Field2", "Field2")
            });

        field1.Should().Be("#Self.#Self.#Self.#Self.#Field1");
        field2.Should().Be("#Self.#Self.#Self.#Field2");
    }

    [Fact]
    public void DuplicatedNavigationProperties_AttributeNames_EnsureUniquness()
    {
        var nametracker = DuplicatedNavigationPropertiesMarshaller.AttributeExpressionNameTracker();
        var field1 = nametracker.Person1.CreatedAt;
        var field2 = nametracker.Person2.CreatedAt;
        var field3 = nametracker.Person1.Address.Name;
        var field4 = nametracker.Person2.Address.Street.Name;

        (nametracker as IAttributeExpressionNameTracker)
            .AccessedNames()
            .Should()
            .BeEquivalentTo(new[]
            {
                new KeyValuePair<string, string>("#CreatedAt", "CreatedAt"),
                new KeyValuePair<string, string>("#Person1", "Person1"),
                new KeyValuePair<string, string>("#Person2", "Person2"),
                new KeyValuePair<string, string>("#Address", "Address"),
                new KeyValuePair<string, string>("#Name", "Name"),
                new KeyValuePair<string, string>("#Street", "Street")
            });

        field1.Should().Be("#Person1.#CreatedAt");
        field2.Should().Be("#Person2.#CreatedAt");
        field3.Should().Be("#Person1.#Address.#Name");
        field4.Should().Be("#Person2.#Address.#Street.#Name");
    }

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
        var nameTracker = ClassWithOverriddenAttributeNameMarshaller.AttributeExpressionNameTracker();
        var valueTracker = ClassWithOverriddenAttributeNameMarshaller.AttributeExpressionValueTracker();

        nameTracker.Foo.Should().Be("#SomethingElse");
        valueTracker.Foo.Should().Be(":p1");
    }

    [Fact]
    public void InheritiedClassWithOverriddenAttributeName_AttributeReferences_ShouldChangeNameValue()
    {
        var nameTracker = InheritedClassMarshaller.AttributeExpressionNameTracker();
        var valueTracker = InheritedClassMarshaller.AttributeExpressionValueTracker();

        nameTracker.Foo.Should().Be("#SomethingElse");
        valueTracker.Foo.Should().Be(":p1");
    }

    [Fact]
    public void Person_AttributeReferences_ShouldBeCorrectlySet()
    {
        var nameTracker = PersonMarshaller.AttributeExpressionNameTracker();
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
    public static IEnumerable<T4> TraverseByValueTracker<T, T2, T3, T4>(this IDynamoDBMarshaller<T, T2, T3, T4> source,
        Func<T4, T4> recursiveSelector, int count)
        where T3 : IAttributeExpressionNameTracker
        where T4 : IAttributeExpressionValueTracker<T2>
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
        where T3 : IAttributeExpressionNameTracker
        where T4 : IAttributeExpressionValueTracker<T2>
    {
        var attributeReferences = source.AttributeExpressionNameTracker();

        for (var i = 0; i < count; i++)
        {
            yield return attributeReferences;

            attributeReferences = recursiveSelector(attributeReferences);
        }
    }
}

public class InheritedClass : ClassWithOverriddenAttributeName
{
}

public class ClassWithOverriddenAttributeName
{
    [DynamoDBProperty("SomethingElse")]
    public string Foo { get; set; } = null!;
}

public class DuplicatedNavigationProperties
{
    public Person Person1 { get; set; } = null!;
    public Person Person2 { get; set; } = null!;
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