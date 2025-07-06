namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Models;

public sealed record Address
{
    public string Id { get; set; } = null!;

    public string? Street { get; set; } = null!;

    public PostalCode PostalCode { get; set; } = null!;

    public List<Person> Neighbours { get; set; } = null!;
}
