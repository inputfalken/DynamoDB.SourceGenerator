using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

// Great source:  https://github.com/dotnet/runtime/blob/main/src/tools/illink/src/ILLink.RoslynAnalyzer/CompilationExtensions.cs
public static class CompilationExtensions
{
    public static ReadOnlySpan<ITypeSymbol> GetTypeSymbols(this Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        var span = classDeclarations.AsSpan();
        var symbols = new ITypeSymbol[classDeclarations.Length];
        for (var i = 0; i < span.Length; i++)
        {
            var classDeclarationSyntax = span[i];

            if (compilation
                    .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classDeclarationSyntax)
                is not ITypeSymbol typeSymbol)
                throw new ArgumentException($"Could not convert the '{classDeclarationSyntax.ToFullString()}' into a '{nameof(ITypeSymbol)}'.");

            symbols[i] = typeSymbol;
        }

        return new ReadOnlySpan<ITypeSymbol>(symbols);
    }
}