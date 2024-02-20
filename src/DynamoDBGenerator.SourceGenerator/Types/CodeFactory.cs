using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Types;

public readonly struct CodeFactory
{
    public CodeFactory(IEnumerable<string> code, IEnumerable<ITypeSymbol> typeIdentifiers)
    {
        _code = code;
        TypeIdentifiers = typeIdentifiers;
    }

    public CodeFactory(IEnumerable<string> code)
    {
        _code = code;
        TypeIdentifiers = Enumerable.Empty<ITypeSymbol>();
    }

    /// <summary>
    ///     The code surrounding all assignments.
    /// </summary>
    private readonly IEnumerable<string> _code;

    /// <summary>
    ///     The assignments that occur within the method.
    /// </summary>
    private IEnumerable<ITypeSymbol> TypeIdentifiers { get; }

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

        foreach (var s in code._code)
            yield return s;

        foreach (var x in code.TypeIdentifiers)
        foreach (var assignment in Create(x, codeSelector, handledTypes))
            yield return assignment;
    }
}