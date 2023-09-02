namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests;

[DynamoDBMarshallert(typeof(WillHaveChangedPropertyName), PropertyName = "SomethingElse")]
public partial class RenameSourceGeneratedPropertyTests
{

    [Fact]
    public void DynamoDBDocumentAttribute_PropertyName_Changes()
    {
        GetType().GetProperties().Should().SatisfyRespectively(x =>
        {
            x.Name.Should().Be("SomethingElse");
        });
    }
}

public class WillHaveChangedPropertyName
{

}