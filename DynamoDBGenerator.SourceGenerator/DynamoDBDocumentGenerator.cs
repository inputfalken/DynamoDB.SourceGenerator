using System.Collections.Immutable;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.DynamoDBDocument;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
// ReSharper disable once InconsistentNaming
public class DynamoDBDocumentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var updateClassDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDbDocumentPropertyFullname,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (ClassDeclarationSyntax)context.TargetNode
            );

        var compilationAndClasses = context.CompilationProvider.Combine(updateClassDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndClasses, Execute);
    }

    // https://github.com/dotnet/runtime/blob/4ea93a6be4ea1b084158cf2aed7cac2414f10a2d/src/libraries/System.Text.Json/gen/JsonSourceGenerator.Roslyn4.0.cs
    private static void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
    {
        var (compilation, documents) = tuple;

        if (documents.IsDefaultOrEmpty)
            return;

        foreach (var typeSymbol in compilation.GetTypeSymbols(documents))
        {
            var repository = string.Join(Constants.NewLine, GetMethods(typeSymbol, compilation));
            var code = typeSymbol.CreateNamespace(typeSymbol.CreateClass(repository));
            context.AddSource(typeSymbol.Name, code);
        }
    }

    private static IEnumerable<string> GetMethods(ISymbol typeSymbol, Compilation compilation)
    {
        var typeNames = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass?.ContainingNamespace is {Name: nameof(DynamoDBGenerator)})
            .Where(x => x.AttributeClass!.Name is nameof(DynamoDBDocument))
            .SelectMany(x => x.ConstructorArguments, (x, y) => (AttributeData: x, TypeConstant: y))
            .Where(x => x.TypeConstant.Kind is TypedConstantKind.Type);

        foreach (var typeName in typeNames)
        {
            var namedTypeSymbol = compilation.GetBestTypeByMetadataName(typeName.TypeConstant.Value!.ToString());
            if (namedTypeSymbol is null)
                continue;

            yield return new DynamoDbDocumentGenerator(namedTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated), SymbolEqualityComparer.IncludeNullability)
                .CreateDynamoDbDocumentProperty(Accessibility.Public);
        }
    }


}