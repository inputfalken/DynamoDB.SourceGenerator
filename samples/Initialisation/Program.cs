// See https://aka.ms/new-console-template for more information

using DynamoDBGenerator.Attributes;

_ = new Constructor(id: "123", count: 10);
_ = new Initializer { Id = "123", Count = 10 };

[DynamoDBMarshaller]
public partial class Constructor
{
    [DynamoDBMarshallerConstructor]
    public Constructor(string id, int count)
    {
        Count = count;
        Id = id;
    }

    public int Count { get; }
    public string Id { get; }
}

[DynamoDBMarshaller]
public partial class Initializer
{
    public string Id { get; set; }
    public int Count { get; set; }
}