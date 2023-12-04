namespace DynamoDB.SourceGenerator.Benchmarks.Models;

public class Address
{
    public string Id { get; set; } = null!;

    public string Street { get; set; } = null!;

    public PostalCode PostalCode { get; set; } = null!;

    public List<PersonEntity> Neighbours { get; set; } = null!;
}