namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Generics;

[DynamoDBDocument(typeof(KeyValueCollectionClass))]
public partial class KeyValueCollectionTests
{

}

public class KeyValueCollectionClass
{
    public List<KeyValuePair<string, int>>? List { get; set; }
    public KeyValuePair<string, int>[]? Array { get; set; }
    public IList<KeyValuePair<string, int>>? ListInterface { get; set; }
    public IReadOnlyList<KeyValuePair<string, int>>? ReadOnlyListInterface { get; set; }
    public ICollection<KeyValuePair<string, int>>? CollectionInterface { get; set; }
    public IReadOnlyCollection<KeyValuePair<string, int>>? ReadOnlyCollectionInterface { get; set; }

}