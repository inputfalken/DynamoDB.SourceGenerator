using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Extensions;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(EntityType = typeof((Guid, Guid)), PropertyName = "STRING", ArgumentType = typeof(string))]
[DynamoDBMarshaller(EntityType = typeof((Guid, Guid)), PropertyName = "GUID", ArgumentType = typeof(Guid))]
[DynamoDBMarshaller(EntityType = typeof((Guid, Guid)), PropertyName = "INT", ArgumentType = typeof(int))]
public partial class PrimitiveArgumentTests
{
    [Fact]
    public void ExpressionValueTracker_STRING_ShouldBeExpandedCorrectly()
    {
        IAttributeExpressionValueTracker<string> valueTracker = STRING.AttributeExpressionValueTracker();
        valueTracker.ToString().Should().Be(":p1");

        valueTracker.AccessedValues("hello").Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
        {
            { ":p1", new AttributeValue { S = "hello" } }
        });
    }

    [Fact]
    public void ExpressionValueTracker_INT_ShouldBeExpandedCorrectly()
    {
        IAttributeExpressionValueTracker<int> valueTracker = INT.AttributeExpressionValueTracker();
        valueTracker.ToString().Should().Be(":p1");

        valueTracker.AccessedValues(2).Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
        {
            { ":p1", new AttributeValue { N = "2" } }
        });
    }

    [Fact]
    public void ExpressionValueTracker_GUID_ShouldBeExpandedCorrectly()
    {
        IAttributeExpressionValueTracker<Guid> valueTracker = GUID.AttributeExpressionValueTracker();
        valueTracker.ToString().Should().Be(":p1");

        var guid = Guid.NewGuid();
        valueTracker.AccessedValues(guid).Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
        {
            { ":p1", new AttributeValue { S = guid.ToString() } }
        });
    }
}
