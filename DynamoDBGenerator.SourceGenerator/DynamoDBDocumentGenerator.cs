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
            var typeNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{typeSymbol.ContainingNamespace}.";
            context.AddSource($"{typeNamespace}{typeSymbol.Name}", code);
        }
    }


    private static IEnumerable<string> GetMethods(ISymbol typeSymbol, Compilation compilation)
    {
        DynamoDBDocumentArguments? ResultSelector(AttributeData attributeData)
        {
            var entityType = attributeData.ConstructorArguments
                .FirstOrDefault(x => x is {Kind: TypedConstantKind.Type, Value: not null});

            if (entityType.IsNull)
                return null;

            var compiledTypeSymbol = compilation.GetBestTypeByMetadataName(entityType.Value!.ToString());
            if (compiledTypeSymbol is null)
                return null;

            var propertyName = attributeData.NamedArguments.FirstOrDefault(x => x.Key is nameof(DynamoDBDocumentAttribute.PropertyName)).Value;
            var argumentType = attributeData.NamedArguments.FirstOrDefault(x => x.Key is nameof(DynamoDBDocumentAttribute.ArgumentType)).Value;

            return new DynamoDBDocumentArguments(
                compiledTypeSymbol,
                propertyName.Value?.ToString() ?? $"{compiledTypeSymbol.Name}Document",
                argumentType is {IsNull: false, Value: not null} ? compilation.GetBestTypeByMetadataName(argumentType.Value.ToString()) : null
            );
        }

        var arguments = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass?.ContainingNamespace is {Name: nameof(DynamoDBGenerator)})
            .Where(x => x.AttributeClass!.Name is nameof(DynamoDBDocumentAttribute))
            .Select(ResultSelector);

        foreach (var argument in arguments)
        {
            if (argument is null)
                continue;

            yield return new DynamoDbDocumentGenerator(argument.Value, SymbolEqualityComparer.IncludeNullability).CreateDynamoDbDocumentProperty(Accessibility.Public);
        }
    }


}

public readonly struct DynamoDBDocumentArguments
{
    public DynamoDBDocumentArguments(INamedTypeSymbol entityTypeSymbol, string propertyName, INamedTypeSymbol? argumentType)
    {
        EntityTypeSymbol = entityTypeSymbol;
        PropertyName = propertyName;
        ArgumentType = argumentType;
    }
    public INamedTypeSymbol EntityTypeSymbol { get; }
    public INamedTypeSymbol? ArgumentType { get; }
    public string PropertyName { get; }
}