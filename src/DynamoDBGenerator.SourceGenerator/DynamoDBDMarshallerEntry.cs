using System.Collections.Immutable;
using System.Diagnostics;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
// ReSharper disable once InconsistentNaming
public class DynamoDBDMarshallerEntry : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var updateClassDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDBGenerator.DynamoDbDocumentPropertyFullname,
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
            var enumerable = typeSymbol.CreateNamespace(typeSymbol.CreateClass(new DynamoDbMarshaller(CreateArguments(typeSymbol)).CreateRepository(), indentLevel: 1));
            var typeNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{typeSymbol.ContainingNamespace}.";


            var code = string.Join(Constants.NewLine, enumerable);
            context.AddSource($"{typeNamespace}{typeSymbol.Name}", code);
        }
    }

    private static IEnumerable<DynamoDBMarshallerArguments> CreateArguments(ISymbol typeSymbol)
    {
        var attributes = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass is
            {
                ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                Name: Constants.DynamoDBGenerator.Attribute.DynamoDBMarshaller,
                ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
            });

        foreach (var attributeData in attributes)
        {
            var entityType = attributeData.ConstructorArguments
                .Select(x => x is {Kind: TypedConstantKind.Type, Value: not null} ? x.Value : null)
                .FirstOrDefault(x => x is not null);

            if (entityType is not INamedTypeSymbol entityTypeSymbol)
                throw new ArgumentException("Could not determine type conversion from attribute constructor.");

            var propertyName = attributeData.NamedArguments.FirstOrDefault(x => x.Key is Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerArgument.PropertyName).Value;
            yield return new DynamoDBMarshallerArguments(
                entityTypeSymbol,
                attributeData.NamedArguments
                    .Where(x => x.Key is Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerArgument.ArgumentType)
                    .Cast<KeyValuePair<string, TypedConstant>?>()
                    .FirstOrDefault() is { } argumentType
                    ? argumentType.Value is
                        {Value : INamedTypeSymbol namedTypeSymbol}
                        ? namedTypeSymbol
                        : throw new ArgumentException($"Could not determine type conversion from argument '{argumentType.Key}'.")
                    : null,
                propertyName.Value?.ToString()
            );

        }

    }

}