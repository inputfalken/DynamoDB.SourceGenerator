using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBMarshaller(EntityType = typeof(ReferenceTypeClass))]
public partial class ReferenceTypeTests
{
    [Fact]
    public void Serialize_ReferenceType_DefaultValueIsNotIncluded()
    {
        var @class = new ReferenceTypeClass();

        ReferenceTypeClassMarshaller
            .Marshall(@class)
            .Should()
            .BeEmpty();
    }
}

public class ReferenceTypeClass
{
    [DynamoDBProperty]
    public string? ReferenceType { get; set; }
}
