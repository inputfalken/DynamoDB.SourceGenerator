using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynamoDBGenerator.SourceGenerator.Types;

/// <summary>
///     A DynamoDB data member that originates from either a field or property.
/// </summary>
public readonly struct DynamoDbDataMember
{
    private const string DynamoDbNameSpace = nameof(Amazon.DynamoDBv2.DataModel);

    public DynamoDbDataMember(DataMember dataMember)
    {
        Attributes = dataMember.BaseSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass is {ContainingAssembly.Name: "AWSSDK.DynamoDBv2"})
            .ToArray();
        IsIgnored = Attributes.Any(x => x.AttributeClass is {Name: "DynamoDBIgnoreAttribute"});
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
                case {AttributeClass.Name: "DynamoDBHashKeyAttribute", ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case {AttributeClass.Name: "DynamoDBRangeKeyAttribute", ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case {AttributeClass.Name: "DynamoDBPropertyAttribute", ConstructorArguments.Length: 1} when
                    FilterString(attributeData.ConstructorArguments[0]) is { } attributeName:
                    return attributeName;
                case
                {
                    AttributeClass.Name: "DynamoDBPropertyAttribute" or "DynamoDBRangeKeyAttribute"
                    or "DynamoDBHashKeyAttribute",
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
            .Where(x => x.Attribute.AttributeClass is {Name: "DynamoDBHashKeyAttribute"})
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();

        if (partitionKey is null)
            return null;

        var rangeKey = items
            .Where(x => x.Attribute.AttributeClass is {Name: "DynamoDBRangeKeyAttribute"})
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();


        var lsi = items
            .Where(x => x.Attribute.AttributeClass is {Name: "DynamoDBLocalSecondaryIndexRangeKeyAttribute"})
            .SelectMany(x => GetStringArrayFromConstructor(x.Attribute),
                (x, y) => new LocalSecondaryIndex(x.DataMember, y!));


        var gsi = items
            .Where(x => x.Attribute.AttributeClass is
            {
                Name: "DynamoDBGlobalSecondaryIndexHashKeyAttribute" or "DynamoDBGlobalSecondaryIndexRangeKeyAttribute"
            })
            .GroupBy(x => x.Attribute.AttributeClass!.Name switch
            {
                "DynamoDBGlobalSecondaryIndexHashKeyAttribute"
                    when GetStringArrayFromConstructor(x.Attribute).FirstOrDefault() is { } index => index,
                "DynamoDBGlobalSecondaryIndexRangeKeyAttribute"
                    when GetStringArrayFromConstructor(x.Attribute).FirstOrDefault() is { } index => index,
                _ => throw new NotSupportedException(x.DataMember.DataMember.Type.ToDisplayString())
            })
            .Select(x =>
            {
                var gsiPartitionKey = x
                    .Where(y => y.Attribute.AttributeClass is {Name: "DynamoDBGlobalSecondaryIndexHashKeyAttribute"})
                    .Select(y => y.DataMember)
                    .Cast<DynamoDbDataMember?>()
                    .FirstOrDefault();

                var gsiRangeKey = x
                    .Where(y => y.Attribute.AttributeClass is {Name: "DynamoDBGlobalSecondaryIndexRangeKeyAttribute"})
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