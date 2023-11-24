using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

/// <summary>
///     A DynamoDB data member that originates from either a field or property.
/// </summary>
public readonly struct DynamoDbDataMember
{
    public DynamoDbDataMember(DataMember dataMember)
    {
        Attributes = dataMember.BaseSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass is {ContainingAssembly.Name: Constants.AWSSDK_DynamoDBv2.AssemblyName})
            .ToArray();
        IsIgnored = Attributes.Any(x => x.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBIgnore});
        AttributeName = ExtractAttributeNameFromAttributes(Attributes) is { } attribute &&
                        string.IsNullOrWhiteSpace(attribute) is false
            ? attribute
            : dataMember.Name;
        DataMember = dataMember;
    }

    private static string? ExtractAttributeNameFromAttributes(IEnumerable<AttributeData> attributes)
    {
        foreach (var attributeData in attributes)
        {
            switch (attributeData)
            {
                case {AttributeClass: null}:
                    continue;
                case {AttributeClass.Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBHashKey, ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case {AttributeClass.Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBRangeKey, ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case {AttributeClass.Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBProperty, ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case
                {
                    AttributeClass.Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBHashKey or Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBRangeKey
                    or Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBProperty,
                    ConstructorArguments.Length: > 0
                }:
                    throw new InvalidOperationException(
                        "Unable to extract attributeName constructor argument from DynamoDBAttributes.");
            }
        }

        return null;
    }

    /// <inheritdoc cref="Types.DataMember" />
    public DataMember DataMember { get; }

    private IReadOnlyList<AttributeData> Attributes { get; }

    /// <summary>
    ///     Indicates whether the property should be ignored being sent to DynamoDB.
    /// </summary>
    public bool IsIgnored { get; }

    public static DynamoDBKeyStructure? GetKeyStructure(IEnumerable<DynamoDbDataMember> members)
    {
        var items = members
            .SelectMany(x => x.Attributes, (x, y) => (DataMember: x, Attribute: y))
            .ToArray();

        var partitionKey = items
            .Where(x => x.Attribute.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBHashKey})
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();

        if (partitionKey is null)
            return null;

        var rangeKey = items
            .Where(x => x.Attribute.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBRangeKey})
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();

        var lsi = items
            .Where(x => x.Attribute.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBLocalSecondaryIndexRangeKey})
            .SelectMany(x => GetStringArrayFromConstructor(x.Attribute),
                (x, y) => new LocalSecondaryIndex(x.DataMember, y!));

        var gsi = items
            .Where(x => x.Attribute.AttributeClass is
            {
                Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexHashKey or Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexRangeKey
            })
            .GroupBy(x => x.Attribute.AttributeClass!.Name switch
            {
                Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexHashKey
                    when GetStringArrayFromConstructor(x.Attribute).FirstOrDefault() is { } index => index,
                Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexRangeKey
                    when GetStringArrayFromConstructor(x.Attribute).FirstOrDefault() is { } index => index,
                _ => throw new NotSupportedException(x.DataMember.DataMember.Type.ToDisplayString())
            })
            .Select(x =>
            {
                var gsiPartitionKey = x
                    .Where(y => y.Attribute.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexHashKey})
                    .Select(y => y.DataMember)
                    .Cast<DynamoDbDataMember?>()
                    .FirstOrDefault();

                var gsiRangeKey = x
                    .Where(y => y.Attribute.AttributeClass is {Name: Constants.AWSSDK_DynamoDBv2.Attribute.DynamoDBGlobalSecondaryIndexRangeKey})
                    .Select(y => y.DataMember)
                    .Cast<DynamoDbDataMember?>()
                    .FirstOrDefault();

                if (gsiPartitionKey is null)
                    throw new NotSupportedException("Could not determine GSI");

                return new GlobalSecondaryIndex(gsiPartitionKey.Value, gsiRangeKey, x.Key);
            });

        return new DynamoDBKeyStructure(partitionKey.Value, rangeKey, lsi, gsi);

        static IEnumerable<string> GetStringArrayFromConstructor(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Select(FilterString).FirstOrDefault(x => x is not null) is { } str)
                return new[] {str};

            return attribute.ConstructorArguments
                .Where(y => y.Kind is TypedConstantKind.Array)
                .Select(y => y.Values
                        .Select(FilterString)
                        .Where(x => x is not null) as IEnumerable<string>
                )
                .FirstOrDefault() ?? Enumerable.Empty<string>();
        }
    }

    private static string? FilterString(TypedConstant constant) =>
        constant is
        {
            Kind: TypedConstantKind.Primitive,
            Value: string str
        }
        && string.IsNullOrWhiteSpace(str) is false
            ? str
            : null;


    /// <summary>
    ///     The name of a DynamoDB attribute
    /// </summary>
    /// <example>
    ///     https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/WorkingWithItems.html
    /// </example>
    public string AttributeName { get; }
}