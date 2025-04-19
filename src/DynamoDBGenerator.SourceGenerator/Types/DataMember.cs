using DynamoDBGenerator.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

/// <summary>
///     Either an field or property.
/// </summary>
public readonly struct DataMember
{
    private DataMember(in ISymbol symbol, in string fieldName, in ITypeSymbol type, in bool isAssignable)
    {
        Name = fieldName;
        BaseSymbol = symbol;
        IsAssignable = isAssignable;
        NameAsPrivateField = fieldName.ToPrivateFieldFromPascal();
        NameAsCamelCase = fieldName.ToCamelCaseFromPascal();
        TypeIdentifier = type.TypeIdentifier();
    }

    public static DataMember FromField(in IFieldSymbol fieldSymbol)
    {
        ISymbol symbol = fieldSymbol;
        var name = fieldSymbol.Name;
        var type = fieldSymbol.Type;

        return new DataMember(in symbol, in name, in type, fieldSymbol.IsReadOnly is false);
    }

    public static DataMember FromProperty(in IPropertySymbol property)
    {
        ISymbol symbol = property;
        var name = property.Name;
        var type = property.Type;

        return new DataMember(in symbol, in name, in type, property.SetMethod?.DeclaredAccessibility is Accessibility.Public);
    }

    /// <summary>
    ///     The <see cref="ISymbol" /> which this instance is based on.
    /// </summary>
    public ISymbol BaseSymbol { get; }

    /// <summary>
    ///     The name of the data member.
    /// </summary>
    public string Name { get; }

    public TypeIdentifier TypeIdentifier { get; }
    public string NameAsPrivateField { get; }
    public string NameAsCamelCase { get; }
    public bool IsAssignable { get; }
}
