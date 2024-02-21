using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct CodeFactory
{
    public CodeFactory(IEnumerable<string> lines, IEnumerable<ITypeSymbol> dependantTypes)
    {
        _lines = lines;
        _dependantTypes = dependantTypes;
    }

    public CodeFactory(IEnumerable<string> lines)
    {
        _lines = lines;
        _dependantTypes = Enumerable.Empty<ITypeSymbol>();
    }

    /// <summary>
    ///     The code lines.
    /// </summary>
    private readonly IEnumerable<string> _lines;

    /// <summary>
    ///     The types that are <see cref="_lines"/> are dependant on.
    /// </summary>
    private readonly IEnumerable<ITypeSymbol> _dependantTypes;

    public static IEnumerable<string> Create(
        ITypeSymbol typeSymbol,
        Func<ITypeSymbol, CodeFactory> codeSelector,
        ISet<ITypeSymbol> handledTypes
    )
    {
        // We already support the type.
        if (handledTypes.Add(typeSymbol) is false)
            yield break;

        var code = codeSelector(typeSymbol);

        foreach (var s in code._lines)
            yield return s;

        foreach (var nestedTypeSymbol in code._dependantTypes)
        foreach (var nestedCode in Create(nestedTypeSymbol, codeSelector, handledTypes))
            yield return nestedCode;
    }
}