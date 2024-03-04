using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof((Guid, Guid)), PropertyName = "PrimitiveArgument", ArgumentType = typeof(string))]
public partial class PrimitiveArgumentTests
{
    [Fact]
    public void ExpressionValueTracker_String_ShouldBeExpandedCorrectly()
    {
        IAttributeExpressionValueTracker<string> valueTracker = PrimitiveArgument.AttributeExpressionValueTracker();
        valueTracker.ToString().Should().Be(":p1");

        valueTracker.AccessedValues("hello").Should().BeEquivalentTo(new Dictionary<string, AttributeValue>
        {
            { ":p1", new AttributeValue { S = "hello" } }
        });
    }
}