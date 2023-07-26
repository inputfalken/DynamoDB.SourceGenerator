namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize;

[DynamoDBDocument(typeof(ReferenceTypeClass))]
public partial class ReferenceTypeTests
{
    [Fact]
    public void Serialize_ReferenceType_DefaultValueIsNotIncluded()
    {
        var @class = new ReferenceTypeClass();

        ReferenceTypeClassDocument
            .Serialize(@class)
            .Should()
            .BeEmpty();
    }
}

public class ReferenceTypeClass
{
    [DynamoDBProperty]
    public string? ReferenceType { get; set; }
}
