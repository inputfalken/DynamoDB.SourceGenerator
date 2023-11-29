using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Conversion
{
    public Conversion(in IEnumerable<string> code, in IEnumerable<Assignment> assignments)
    {
        Code = code;
        Assignments = assignments;
    }
    /// <summary>
    ///     The code surrounding all assignments.
    /// </summary>
    public IEnumerable<string> Code { get; }

    /// <summary>
    ///     The assignments that occur within the method.
    /// </summary>
    private IEnumerable<Assignment> Assignments { get; }

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

        foreach (var unknownAssignment in rootConversion.Assignments.Select(x => x.TypeIdentifier).OfType<UnknownType>())
        {
            foreach (var assignment in ConversionMethods(unknownAssignment.TypeSymbol, conversionSelector, typeSymbols))
                yield return assignment;
        }
    }
}