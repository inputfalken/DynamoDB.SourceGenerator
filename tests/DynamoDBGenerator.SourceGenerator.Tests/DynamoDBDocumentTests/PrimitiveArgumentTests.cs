using DynamoDBGenerator.Attributes;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshaller(typeof((Guid, Guid)), PropertyName = "PrimitiveArgument", ArgumentType = typeof(string))]
public partial class PrimitiveArgumentTests
{
    
    [Fact]
    public void Test()
    {
        PrimitiveArgument.
    }
}