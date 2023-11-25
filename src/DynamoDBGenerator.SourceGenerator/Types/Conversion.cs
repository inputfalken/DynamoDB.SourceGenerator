using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly record struct Conversion
{
    public Conversion(in IEnumerable<string> code, in IEnumerable<Assignment> assignments)
    {
        Code = code;
        Assignments = assignments;
    }
    public Conversion(in string code, in IEnumerable<Assignment> assignments)
    {
        Code = Enumerable.Repeat(code, 1);
        Assignments = assignments;
    }

    /// <summary>
    /// The code surrounding all assignments.
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

        foreach (var unknownAssignment in rootConversion.Assignments.Where(x => x.KnownType is null))
        {
            foreach (var assignment in ConversionMethods(unknownAssignment.Type, conversionSelector, typeSymbols))
                yield return assignment;
        }
    }
    public void Deconstruct(out IEnumerable<string> Code, out IEnumerable<Assignment> Assignments)
    {
        Code = this.Code;
        Assignments = this.Assignments;
    }
}