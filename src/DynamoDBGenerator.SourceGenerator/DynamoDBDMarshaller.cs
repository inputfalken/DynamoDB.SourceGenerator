using System.Collections.Immutable;
using System.Diagnostics;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerArgument;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptionsArgument;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
// ReSharper disable once InconsistentNaming
public class DynamoDBDMarshaller : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var updateClassDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDBGenerator.DynamoDBMarshallerFullname,
                (n, _) => n is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax,
                (c, _) => c.TargetNode
            );

        var compilationAndClasses = context.CompilationProvider.Combine(updateClassDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndClasses, Execute);
    }

    // https://github.com/dotnet/runtime/blob/4ea93a6be4ea1b084158cf2aed7cac2414f10a2d/src/libraries/System.Text.Json/gen/JsonSourceGenerator.Roslyn4.0.cs
    private static void Execute(SourceProductionContext context,
        (Compilation Left, ImmutableArray<SyntaxNode> Right) tuple)
    {
        var (compilation, classDeclarationSyntax) = tuple;

        if (classDeclarationSyntax.IsDefaultOrEmpty)
            return;

        foreach (var typeSymbol in compilation.GetTypeSymbols(classDeclarationSyntax))
        {
            var (options, args) = CreateArguments(typeSymbol, compilation);
            context.AddSource(
                $"{typeSymbol.ToDisplayString()}.g",
                string.Join(Constants.NewLine, MarshallerFactory.Create(typeSymbol, args.ToArray(), options))
            );
        }
    }

    private static (MarshallerOptions, IEnumerable<DynamoDBMarshallerArguments>) CreateArguments(INamedTypeSymbol type,
        Compilation compilation)
    {
        var attributes = type.GetAttributes();
        var marshallerOptionNamedArguments = attributes
            .Where(x => x.AttributeClass is
            {
                Name: Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptions,
                ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
            })
            .SelectMany(x => x.NamedArguments)
            .ToArray();

        var enumStrategy = marshallerOptionNamedArguments
            .Where(x => x.Key is EnumConversionStrategy)
            .Where(x => x.Value.Kind is TypedConstantKind.Enum)
            .Select(x => x.Value.Value)
            .OfType<int?>()
            .FirstOrDefault(x => x is not null) ?? ConversionStrategy.Integer;

        var converter = marshallerOptionNamedArguments
                            .Where(x => x.Key is Converters)
                            .Select(x => x.Value.Value)
                            .OfType<INamedTypeSymbol>()
                            .FirstOrDefault(x => x is not null) ??
                        compilation.GetTypeByMetadataName(Constants.DynamoDBGenerator.DynamoDBConverterFullName);

        if (converter is null)
            throw new ArgumentException("Could not find converter implementation");

        return (MarshallerOptions.Create(type, converter, enumStrategy), Arguments(attributes, type));

        static IEnumerable<DynamoDBMarshallerArguments> Arguments(
            ImmutableArray<AttributeData> attributes,
            ISymbol type)
        {
            return attributes
                .Where(attributeData => attributeData.AttributeClass is
                {
                    ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                    Name: Constants.DynamoDBGenerator.Attribute.DynamoDBMarshaller,
                    ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
                })
                .Select(x =>
                {
                    var entityType = x.NamedArguments
                        .Where(x => x.Key is EntityType)
                        .Cast<KeyValuePair<string, TypedConstant>?>()
                        .FirstOrDefault() is { } entityType1
                        ? entityType1.Value is { Value: INamedTypeSymbol et }
                            ? et
                            : throw new ArgumentException(
                                $"Could not determine type conversion from argument '{entityType1.Key}'.")
                        : type;

                    if (entityType is not INamedTypeSymbol entityTypeSymbol)
                        throw new ArgumentException("Could not determine type conversion from attribute constructor.");

                    var propertyName = x.NamedArguments.FirstOrDefault(y => y.Key is AccessName).Value;

                    return new DynamoDBMarshallerArguments(
                        entityTypeSymbol,
                        x.NamedArguments
                            .Where(x => x.Key is ArgumentType)
                            .Cast<KeyValuePair<string, TypedConstant>?>()
                            .FirstOrDefault() is { } argumentType
                            ? argumentType.Value is { Value: INamedTypeSymbol namedTypeSymbol }
                                ? namedTypeSymbol
                                : throw new ArgumentException(
                                    $"Could not determine type conversion from argument '{argumentType.Key}'.")
                            : null,
                        propertyName.Value?.ToString()
                    );
                });
        }
    }
}