namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Generics;

public class LookUpTests
{
    
}
[AttributeValueGenerator]
public partial class LookUpClass
{
    [DynamoDBProperty]
    public ILookup<string, int>? Lookup { get; set; }

}
