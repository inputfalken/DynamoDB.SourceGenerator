using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

/// <summary>
/// Either an field or property.
/// </summary>
public readonly struct DataMember
{
    private readonly bool _isField;
    private readonly bool _isProperty;

    private DataMember(in ISymbol symbol, in string fieldName, in ITypeSymbol type)
    {
        Name = fieldName;
        Type = type;
        BaseSymbol = symbol;

        _isField = symbol is IFieldSymbol;
        _isProperty = symbol is IPropertySymbol;
    }

    public static DataMember FromField(in IFieldSymbol fieldSymbol)
    {
        var symbol = (ISymbol) fieldSymbol;
        var name = fieldSymbol.Name;
        var type = fieldSymbol.Type;

        return new DataMember(in symbol, in name, in type);
    }

    public static DataMember FromProperty(in IPropertySymbol property)
    {
        var symbol = (ISymbol) property;
        var name = property.Name;
        var type = property.Type;

        return new DataMember(in symbol, in name, in type);
    }

    /// <summary>
    /// Performs matching based on the possible types that <see cref="DataMember"/> can consist of.
    /// </summary>
    public T Match<T>(Func<IPropertySymbol, T> propertySelector, Func<IFieldSymbol, T> fieldSelector)
    {
        if (_isProperty)
            return propertySelector((IPropertySymbol) BaseSymbol);

        if (_isField)
            return fieldSelector((IFieldSymbol) BaseSymbol);

        throw new Exception("Can never happen");
    }

    /// <summary>
    /// The <see cref="ISymbol"/> which this instance is based on.
    /// </summary>
    public ISymbol BaseSymbol { get; }

    /// <summary>
    /// The name of the data member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of the data member.
    /// </summary>
    public ITypeSymbol Type { get; }
}

/// <summary>
///  A DynamoDB data member that originates from either a field or property.
/// </summary>
public readonly struct DynamoDbDataMember
{
    private const string DynamoDbNameSpace = nameof(Amazon.DynamoDBv2.DataModel);

    public DynamoDbDataMember(DataMember dataMember)
    {
        var attributes = GetAttributes(dataMember.BaseSymbol).ToArray();

        IsIgnored = attributes.OfType<DynamoDBIgnoreAttribute>().Any();
        IsHashKey = attributes.OfType<DynamoDBHashKeyAttribute>().Any();
        IsRangeKey = attributes.OfType<DynamoDBRangeKeyAttribute>().Any();
        AttributeName = attributes
            .OfType<DynamoDBRenamableAttribute>()
            .FirstOrDefault()?.AttributeName ?? dataMember.Name;
        DataMember = dataMember;
    }


    ///<inheritdoc cref="DynamoDBGenerator.SourceGenerator.DataMember"/>
    public DataMember DataMember { get; }

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