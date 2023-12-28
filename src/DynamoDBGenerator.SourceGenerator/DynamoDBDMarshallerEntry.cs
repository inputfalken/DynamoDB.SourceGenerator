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
                (n, _) => n is ClassDeclarationSyntax,
                (c, _) => (ClassDeclarationSyntax)c.TargetNode
            );

        var compilationAndClasses = context.CompilationProvider.Combine(updateClassDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndClasses, Execute);
    }

    // https://github.com/dotnet/runtime/blob/4ea93a6be4ea1b084158cf2aed7cac2414f10a2d/src/libraries/System.Text.Json/gen/JsonSourceGenerator.Roslyn4.0.cs
    private static void Execute(SourceProductionContext context,
        (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
    {
        var (compilation, classDeclarationSyntax) = tuple;

        if (classDeclarationSyntax.IsDefaultOrEmpty)
            return;

        foreach (var typeSymbol in compilation.GetTypeSymbols(classDeclarationSyntax))
        {
            var typeNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{typeSymbol.ContainingNamespace}.";

            context.AddSource($"{typeNamespace}{typeSymbol.Name}.g",
                string.Join(Constants.NewLine, CreateFileContent(typeSymbol, compilation)));
        }
    }

    private static IEnumerable<string> CreateFileContent(ISymbol type, Compilation compilation)
    {
        var timestamp = Stopwatch.GetTimestamp();
        yield return $@"// <auto-generated | TimeStamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}>
#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using {Constants.AWSSDK_DynamoDBv2.Namespace.ModelFullName};
using {Constants.DynamoDBGenerator.Namespace.Root};
using {Constants.DynamoDBGenerator.Namespace.AttributesFullName};
using {Constants.DynamoDBGenerator.Namespace.ExceptionsFullName};
using {Constants.DynamoDBGenerator.Namespace.ConvertersFullName};
using {Constants.DynamoDBGenerator.Namespace.InternalFullName};";

        var (options, args) = CreateArguments(type, compilation);
        var classContent =
            $"public sealed partial class {type.Name}".CreateBlock(DynamoDbMarshaller.CreateRepository(args, options));
        var content = type.ContainingNamespace.IsGlobalNamespace
            ? classContent
            : $"namespace {type.ContainingNamespace.ToDisplayString()}".CreateBlock(classContent);

        foreach (var s in content)
            yield return s;

        var duration = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp);

        yield return $"// <auto-generated | Duration {duration.ToString()}>";
    }

    private static (MarshallerOptions, IEnumerable<DynamoDBMarshallerArguments>) CreateArguments(ISymbol type,
        Compilation compilation)
    {
        var attributes = type.GetAttributes();
        var converter = attributes
                            .Where(x => x.AttributeClass is
                            {
                                Name: Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptions,
                                ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                                ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
                            })
                            .SelectMany(x => x.NamedArguments)
                            .Where(x => x.Key is Constants.DynamoDBGenerator.Attribute.DynamoDbMarshallerOptionsArgument.Converters)
                            .Select(x => x.Value.Value)
                            .OfType<INamedTypeSymbol>()
                            .FirstOrDefault(x => x is not null) ??
                        compilation.GetTypeByMetadataName(Constants.DynamoDBGenerator.DynamoDBConverterFullName);

        if (converter is null)
            throw new ArgumentException("Could not find converter implementation");

        return (MarshallerOptions.Create(converter), Arguments(attributes));

        static IEnumerable<DynamoDBMarshallerArguments> Arguments(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attributeData in attributes)
            {
                if (attributeData.AttributeClass is
                    {
                        ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                        Name: Constants.DynamoDBGenerator.Attribute.DynamoDBMarshaller,
                        ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
                    } is false)
                    continue;

                var entityType = attributeData.ConstructorArguments
                    .Select(x => x is { Kind: TypedConstantKind.Type, Value: not null } ? x.Value : null)
                    .FirstOrDefault(x => x is not null);

                if (entityType is not INamedTypeSymbol entityTypeSymbol)
                    throw new ArgumentException("Could not determine type conversion from attribute constructor.");

                var propertyName = attributeData.NamedArguments.FirstOrDefault(x =>
                    x.Key is Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerArgument.PropertyName).Value;
                yield return new DynamoDBMarshallerArguments(
                    entityTypeSymbol,
                    attributeData.NamedArguments
                        .Where(x => x.Key is Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerArgument
                            .ArgumentType)
                        .Cast<KeyValuePair<string, TypedConstant>?>()
                        .FirstOrDefault() is { } argumentType
                        ? argumentType.Value is
                            { Value : INamedTypeSymbol namedTypeSymbol }
                            ? namedTypeSymbol
                            : throw new ArgumentException(
                                $"Could not determine type conversion from argument '{argumentType.Key}'.")
                        : null,
                    propertyName.Value?.ToString()
                );
            }
        }
    }
}