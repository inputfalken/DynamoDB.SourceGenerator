using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct RootEntity
{
    public RootEntity(INamedTypeSymbol entity, DynamoDBKeyStructure? keyStructure)
    {
        Entity = entity;
        KeyStructure = keyStructure;
    }

    /// <summary>
    /// Represents the root entity that all functionality will be based from.
    /// </summary>
    public INamedTypeSymbol Entity { get; }

    /// <summary>
    /// Represents the DynamoDB keyStructure.
    /// </summary>
    public DynamoDBKeyStructure? KeyStructure { get; }
}

/// <summary>
/// Represents the key structure of a DynamoDB table.
/// </summary>
public readonly struct DynamoDBKeyStructure
{
    public DynamoDBKeyStructure(DynamoDbDataMember partitionKey, DynamoDbDataMember? sortKey, IReadOnlyList<LocalSecondaryIndex> localSecondaryIndices, IReadOnlyList<Amazon.DynamoDBv2.Model.GlobalSecondaryIndex> globalSecondaryIndices)
    {
        PartitionKey = partitionKey;
        SortKey = sortKey;
        LocalSecondaryIndices = localSecondaryIndices;
        GlobalSecondaryIndices = globalSecondaryIndices;
    }

    /// <summary>
    /// Represents the primary partition key.
    /// </summary>
    public DynamoDbDataMember PartitionKey { get; }

    /// <summary>
    /// Represents the primary sort key.
    /// </summary>
    public DynamoDbDataMember? SortKey { get; }

    /// <summary>
    /// Represents 0 - N amount of LSI.
    /// </summary>
    public IReadOnlyList<LocalSecondaryIndex> LocalSecondaryIndices { get; }

    /// <summary>
    /// Represents 0 - N amount of GSI.
    /// </summary>
    public IReadOnlyList<Amazon.DynamoDBv2.Model.GlobalSecondaryIndex> GlobalSecondaryIndices { get; }
}

/// <summary>
/// https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/LSI.html
/// </summary>
public readonly struct LocalSecondaryIndex
{
    public LocalSecondaryIndex(DynamoDbDataMember sortKey, string name)
    {
        SortKey = sortKey;
        Name = name;
    }

    /// <summary>
    /// Represents the sort key of a LSI.
    /// </summary>
    public DynamoDbDataMember SortKey { get; }

    /// <summary>
    /// Represents the name of the LSI.
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/GSI.html
/// </summary>
public readonly struct GlobalSecondaryIndex
{
    public GlobalSecondaryIndex(DynamoDbDataMember partitionKey, DynamoDbDataMember? sortKey, string name)
    {
        PartitionKey = partitionKey;
        SortKey = sortKey;
        Name = name;
    }

    /// <summary>
    /// Represents the partition key of a GSI.
    /// </summary>
    public DynamoDbDataMember PartitionKey { get; }

    /// <summary>
    /// Represents the sort key of a GSI.
    /// </summary>
    public DynamoDbDataMember? SortKey { get; }

    /// <summary>
    /// Represents the name of the GSI.
    /// </summary>
    public string Name { get; }
}