using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator("Abc")]
public partial class PersonEntity
{

    public (int X, int Y, int Z) Coordinate { get; set; }

}