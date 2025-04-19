using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

// Great source:  https://github.com/dotnet/runtime/blob/main/src/tools/illink/src/ILLink.RoslynAnalyzer/CompilationExtensions.cs
public static class CompilationExtensions
{
    public static ReadOnlySpan<INamedTypeSymbol> GetTypeSymbols(this Compilation compilation,
        ImmutableArray<SyntaxNode> classDeclarations)
    {
        var span = classDeclarations.AsSpan();
        var symbols = new INamedTypeSymbol[classDeclarations.Length];
        for (var i = 0; i < span.Length; i++)
        {
            var classDeclarationSyntax = span[i];

            if (compilation
                    .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classDeclarationSyntax)
                is not INamedTypeSymbol typeSymbol)
                throw new ArgumentException(
                    $"Could not convert the '{classDeclarationSyntax.ToFullString()}' into a '{nameof(ITypeSymbol)}'.");

            symbols[i] = typeSymbol;
        }

        return new ReadOnlySpan<INamedTypeSymbol>(symbols);
    }
}