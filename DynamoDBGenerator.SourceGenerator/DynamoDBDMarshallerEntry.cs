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
            var repository = string.Join(Constants.NewLine, GetMethods(typeSymbol, compilation));
            var code = typeSymbol.CreateNamespace(typeSymbol.CreateClass(repository), TimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp));
            var typeNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{typeSymbol.ContainingNamespace}.";
            context.AddSource($"{typeNamespace}{typeSymbol.Name}", code);
        }
    }


    private static IEnumerable<string> GetMethods(ISymbol typeSymbol, Compilation compilation)
    {
        var attributes = typeSymbol
            .GetAttributes()
            .Where(x => x.AttributeClass is
            {
                ContainingNamespace.Name: Constants.AttributeNameSpace,
                Name: Constants.MarshallerAttributeName,
                ContainingAssembly.Name: Constants.AssemblyName
            });

        return CreateArguments(attributes, compilation)
            .Select(x => new DynamoDbMarshaller(x).CreateDynamoDbDocumentProperty(Accessibility.Public));
    }

    // If we can find duplicated types, we could access their properties instead of duplicating the source generator. 
    // For example :
    // [DynamoDBDocument(typeof(Person))]
    // [DynamoDBDocument(typeof(Person), ArgumentType = typeof(ChangeName))]
    // With this scenario we would be able to use the Person type from the second attribute instead of source generating duplicated code.
    private static IEnumerable<DynamoDBMarshallerArguments> CreateArguments(IEnumerable<AttributeData> attributes, Compilation compilation)
    {
        foreach (var attributeData in attributes)
        {
            var entityType = attributeData.ConstructorArguments
                .FirstOrDefault(x => x is {Kind: TypedConstantKind.Type, Value: not null});

            if (entityType.Value is null || entityType.IsNull)
                continue;

            var qualifiedMetadataName = entityType.Value.ToString();

            var entityTypeSymbol = compilation.GetBestTypeByMetadataName(qualifiedMetadataName);

            if (entityTypeSymbol is null)
                continue;

            var propertyName = attributeData.NamedArguments.FirstOrDefault(x => x.Key is nameof(DynamoDBMarshallerAttribute.PropertyName)).Value;
            var argumentTypeSymbol = attributeData.NamedArguments.FirstOrDefault(x => x.Key is nameof(DynamoDBMarshallerAttribute.ArgumentType)).Value;

            // When a `ValueTuple` arrives we get the following format `(T name, T2 otherName)` for example `(string name, int age)`
            // To support this would be tricky but possible.
            // We could produce and compile a custom type, take the tuple string and use it as the constructor arguments.
            // like `private readonly record struct {SomeString} ({tupleString})` for example `private readonly record struct 5604E01E_9058_4A53_BCC4_B5A0FC1038F9(string name, int age)`;
            // We would publicly accept the ValueTuple and internally build up conversion from compiled type.
            yield return new DynamoDBMarshallerArguments(
                entityTypeSymbol,
                argumentTypeSymbol is {IsNull: false, Value: not null}
                    ? compilation.GetBestTypeByMetadataName(argumentTypeSymbol.Value.ToString())
                    : null,
                propertyName.Value?.ToString(),
                SymbolEqualityComparer.IncludeNullability
            );

        }

    }

}