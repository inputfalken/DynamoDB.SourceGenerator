using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Conversion(in IEnumerable<string> Code, in IEnumerable<Assignment> Assignments)
{
    /// <summary>
    /// The code surrounding all assignments.
    /// </summary>
    public IEnumerable<string> Code { get; } = Code;

    /// <summary>
    ///     The assignments that occur within the method.
    /// </summary>
    private IEnumerable<Assignment> Assignments { get; } = Assignments;

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

        foreach (var unknownAssignment in rootConversion.Assignments.Where(x => x.KnownType is null))
        {
            foreach (var assignment in ConversionMethods(unknownAssignment.Type, conversionSelector, typeSymbols))
                yield return assignment;
        }
    }
}