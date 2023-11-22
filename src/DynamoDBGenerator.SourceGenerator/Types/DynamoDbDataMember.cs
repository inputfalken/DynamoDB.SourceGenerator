using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

/// <summary>
///     A DynamoDB data member that originates from either a field or property.
/// </summary>
public readonly struct DynamoDbDataMember
{
    private const string DynamoDbNameSpace = nameof(Amazon.DynamoDBv2.DataModel);

    public DynamoDbDataMember(DataMember dataMember)
    {
        Attributes = GetAttributes(dataMember.BaseSymbol).ToArray();
        var attributeDatas = dataMember.BaseSymbol.GetAttributes();

        IsIgnored = attributeDatas.Any(x => x.AttributeClass is
        {
            Name: "DynamoDBIgnoreAttribute",
            ContainingAssembly.Name: "AWSSDK.DynamoDBv2"
        });
        AttributeName = Attributes
            .OfType<DynamoDBRenamableAttribute>()
            .FirstOrDefault(x => string.IsNullOrWhiteSpace(x.AttributeName) is false)?.AttributeName ?? dataMember.Name;
        DataMember = dataMember;
    }


    /// <inheritdoc cref="Types.DataMember" />
    public DataMember DataMember { get; }

    public IReadOnlyList<DynamoDBAttribute> Attributes { get; }

    /// <summary>
    ///     Indicates whether the property should be ignored being sent to DynamoDB.
    /// </summary>
    public bool IsIgnored { get; }

    /// <summary>
    ///     The name of a DynamoDB attribute
    /// </summary>
    /// <example>
    ///     https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/WorkingWithItems.html
    /// </example>
    public string AttributeName { get; }

    private static IEnumerable<DynamoDBAttribute> GetAttributes(ISymbol symbol)
    {
        static DynamoDBAttribute? CreateInstance<T>(AttributeData attributeData)
            where T : DynamoDBAttribute => attributeData.CreateInstance<T>();

        var dynamoDbPropertyAttributes = symbol
            .GetAttributes()
            .Where(y => y.AttributeClass?.ContainingNamespace.Name is DynamoDbNameSpace)
            .Where(x =>
            {
                var attributeClass = x.AttributeClass;
                while (attributeClass != null)
                {
                    if (attributeClass is {Name: nameof(DynamoDBAttribute)})
                        return true;

                    attributeClass = attributeClass.BaseType;
                }

                return false;
            });

        foreach (var propertyAttribute in dynamoDbPropertyAttributes)
        {
            if (CreateInstance<DynamoDBHashKeyAttribute>(propertyAttribute) is { } hashKey)
                yield return hashKey;

            if (CreateInstance<DynamoDBRangeKeyAttribute>(propertyAttribute) is { } rangeKey)
                yield return rangeKey;

            if (CreateInstance<DynamoDBGlobalSecondaryIndexHashKeyAttribute>(propertyAttribute) is { } gsiHashKey)
                yield return gsiHashKey;

            if (CreateInstance<DynamoDBGlobalSecondaryIndexRangeKeyAttribute>(propertyAttribute) is { } gsiRangeKey)
                yield return gsiRangeKey;

            if (CreateInstance<DynamoDBLocalSecondaryIndexRangeKeyAttribute>(propertyAttribute) is { } lsiRangeKey)
                yield return lsiRangeKey;

            if (CreateInstance<DynamoDBPropertyAttribute>(propertyAttribute) is { } ddbProperty)
                yield return ddbProperty;

            if (CreateInstance<DynamoDBIgnoreAttribute>(propertyAttribute) is { } ignore)
                yield return ignore;
        }
    }
}