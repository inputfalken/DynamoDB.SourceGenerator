using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

// Great source:  https://github.com/dotnet/runtime/blob/main/src/tools/illink/src/ILLink.RoslynAnalyzer/CompilationExtensions.cs
public static class CompilationExtensions
{
    private static ITypeSymbol? GetTypeSymbol(this Compilation compilation, SyntaxNode typeDeclarationSyntax)
    {
        return compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(typeDeclarationSyntax) as ITypeSymbol;
    }

    public static IEnumerable<ITypeSymbol> GetTypeSymbols(this Compilation compilation, IEnumerable<TypeDeclarationSyntax> typeDeclarationSyntax)
    {
        return typeDeclarationSyntax.Select(x => GetTypeSymbol(compilation, x)).Where(x => x is not null) as IEnumerable<ITypeSymbol>;
    }
}