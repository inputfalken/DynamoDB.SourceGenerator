using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Conversion
{
    public Conversion(IEnumerable<string> code, IEnumerable<ITypeSymbol> typeIdentifiers)
    {
        Code = code;
        TypeIdentifiers = typeIdentifiers;
    }
    
    public Conversion(IEnumerable<string> code)
    {
        Code = code;
        TypeIdentifiers = Enumerable.Empty<ITypeSymbol>();
    }

    /// <summary>
    ///     The code surrounding all assignments.
    /// </summary>
    public IEnumerable<string> Code { get; }

    /// <summary>
    ///     The assignments that occur within the method.
    /// </summary>
    private IEnumerable<ITypeSymbol> TypeIdentifiers { get; }

    public static IEnumerable<Conversion> ConversionMethods(
        ITypeSymbol typeSymbol,
        Func<ITypeSymbol, Conversion> conversionSelector,
        ISet<ITypeSymbol> typeSymbols
    )
    {
        // We already support the type.
        if (typeSymbols.Add(typeSymbol) is false)
            yield break;

        var rootConversion = conversionSelector(typeSymbol);

        yield return rootConversion;

        foreach (var assignment in rootConversion.TypeIdentifiers .SelectMany(x => ConversionMethods(x, conversionSelector, typeSymbols)))
            yield return assignment;
    }
}