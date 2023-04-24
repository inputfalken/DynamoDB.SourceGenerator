using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

/// <summary>
///  A property that DynamoDB needs to handle.
/// </summary>
public readonly struct DynamoDbProperty
{
    private const string DynamoDbNameSpace = nameof(Amazon.DynamoDBv2.DataModel);

    public DynamoDbProperty(IPropertySymbol property)
    {
        var attributes = GetAttributes(property).ToArray();

        Property = property;
        IsIgnored = attributes.OfType<DynamoDBIgnoreAttribute>().Any();
        IsHashKey = attributes.OfType<DynamoDBHashKeyAttribute>().Any();
        IsRangeKey = attributes.OfType<DynamoDBRangeKeyAttribute>().Any();
        AttributeName = attributes
            .OfType<DynamoDBRenamableAttribute>()
            .FirstOrDefault()?.AttributeName ?? property.Name;
    }

    /// <summary>
    /// Indicates whether the property is the HashKey.
    /// </summary>
    public bool IsHashKey { get; }

    /// <summary>
    /// Indicates whether the property is the RangeKey.
    /// </summary>
    public bool IsRangeKey { get; }

    /// <summary>
    /// Indicates whether the property should be ignored being sent to DynamoDB.
    /// </summary>
    public bool IsIgnored { get; }

    /// <summary>
    /// The name of a DynamoDB attribute
    /// </summary>
    /// <example>
    /// https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/WorkingWithItems.html
    /// </example>
    public string AttributeName { get; }

    /// <summary>
    /// A .NET property.
    /// </summary>
    public IPropertySymbol Property { get; }

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

            if (CreateInstance<DynamoDBPropertyAttribute>(propertyAttribute) is { } ddbProperty)
                yield return ddbProperty;

            if (CreateInstance<DynamoDBIgnoreAttribute>(propertyAttribute) is { } ignore)
                yield return ignore;
        }
    }
}