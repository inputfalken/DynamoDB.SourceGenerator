using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using DynamoDBGenerator.Attributes;
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
            var timestamp = Stopwatch.GetTimestamp();
            var repository = new DynamoDbMarshaller(CreateArguments(typeSymbol, compilation), SymbolEqualityComparer.IncludeNullability).CreateDynamoDbDocumentProperty();
            var code = typeSymbol.CreateNamespace(typeSymbol.CreateClass(repository), TimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp));
            var typeNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{typeSymbol.ContainingNamespace}.";
            context.AddSource($"{typeNamespace}{typeSymbol.Name}", code);
        }
    }


    // If we can find duplicated types, we could access their properties instead of duplicating the source generator. 
    // For example :
    // [DynamoDBDocument(typeof(Person))]
    // [DynamoDBDocument(typeof(Person), ArgumentType = typeof(ChangeName))]
    // With this scenario we would be able to use the Person type from the second attribute instead of source generating duplicated code.
    private static IEnumerable<DynamoDBMarshallerArguments> CreateArguments(ISymbol typeSymbol, Compilation compilation)
    {
        var attributes = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass is
            {
                ContainingNamespace.Name: Constants.AttributeNameSpace,
                Name: Constants.MarshallerAttributeName,
                ContainingAssembly.Name: Constants.AssemblyName
            });

        foreach (var attributeData in attributes)
        {
            var entityType = attributeData.ConstructorArguments
                .Select(x => x is {Kind: TypedConstantKind.Type, Value: not null} ? x.Value : null)
                .FirstOrDefault(x => x is not null);

            if (entityType is not INamedTypeSymbol entityTypeSymbol)
                throw new ArgumentException("Could not determine type conversion from attribute constructor.");

            var propertyName = attributeData.NamedArguments.FirstOrDefault(x => x.Key is nameof(DynamoDBMarshallerAttribute.PropertyName)).Value;
            yield return new DynamoDBMarshallerArguments(
                entityTypeSymbol,
                attributeData.NamedArguments
                    .Where(x => x.Key is nameof(DynamoDBMarshallerAttribute.ArgumentType))
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