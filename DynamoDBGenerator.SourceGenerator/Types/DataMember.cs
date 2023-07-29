using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

/// <summary>
///     Either an field or property.
/// </summary>
public readonly struct DataMember
{
    private readonly bool _isField;
    private readonly bool _isProperty;

    private DataMember(in ISymbol symbol, in string fieldName, in ITypeSymbol type, in bool isAssignable)
    {
        Name = fieldName;
        Type = type;
        BaseSymbol = symbol;
        _isField = symbol is IFieldSymbol;
        _isProperty = symbol is IPropertySymbol;
        IsAssignable = isAssignable;
    }

    public static DataMember FromField(in IFieldSymbol fieldSymbol)
    {
        var symbol = (ISymbol) fieldSymbol;
        var name = fieldSymbol.Name;
        var type = fieldSymbol.Type;

        return new DataMember(in symbol, in name, in type, fieldSymbol.IsReadOnly is false);
    }

    public static DataMember FromProperty(in IPropertySymbol property)
    {
        var symbol = (ISymbol) property;
        var name = property.Name;
        var type = property.Type;

        return new DataMember(in symbol, in name, in type, property.SetMethod?.DeclaredAccessibility is Accessibility.Public);
    }

    /// <summary>
    ///     Performs matching based on the possible types that <see cref="DataMember" /> can consist of.
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
    ///     The <see cref="ISymbol" /> which this instance is based on.
    /// </summary>
    public ISymbol BaseSymbol { get; }

    /// <summary>
    ///     The name of the data member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The type of the data member.
    /// </summary>
    public ITypeSymbol Type { get; }
    public bool IsAssignable { get; }
}