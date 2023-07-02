using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

[DynamoDbDocument]
public partial class Address
{
    public string Id { get; set; } = null!;

    public string Street { get; set; } = null!;

    public PostalCode PostalCode { get; set; } = null!;

    public IReadOnlyList<PersonEntity> Neighbours { get; set; }

}