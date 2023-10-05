using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Conversion(
    in string Code,
    in IEnumerable<Assignment> Assignments)
{
    public string Code { get; } = Code;

    /// <summary>
    ///     The assignments that occur within the method.
    /// </summary>
    public IEnumerable<Assignment> Assignments { get; } = Assignments;

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

        foreach (var assignment in rootConversion.Assignments)
        {
            foreach (var innerAssignment in ConversionMethods(assignment.Type, conversionSelector, typeSymbols))
                yield return innerAssignment;
        }
    }
}