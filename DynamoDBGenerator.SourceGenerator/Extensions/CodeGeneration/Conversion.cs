using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;

public readonly record struct Conversion(
    in string Code,
    in IEnumerable<Assignment> Conversions)
{
    /// <summary>
    ///     The C# code for the <see cref="Amazon.DynamoDBv2.Model.AttributeValue" /> conversion.
    /// </summary>
    public string Code { get; } = Code;

    /// <summary>
    ///     The conversions that occur within the method.
    /// </summary>
    public IEnumerable<Assignment> Conversions { get; } = Conversions;

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

        foreach (var conversion in rootConversion.Conversions)
        {
            foreach (var recursionConversion in ConversionMethods(conversion.Type, conversionSelector, typeSymbols))
                yield return recursionConversion;
        }
    }
}