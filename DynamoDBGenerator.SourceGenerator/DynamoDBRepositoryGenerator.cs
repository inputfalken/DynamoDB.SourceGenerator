using System.Collections.Immutable;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
// ReSharper disable once InconsistentNaming
public class DynamoDBRepositoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var putClassDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDBPutOperationFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (ClassDeclarationSyntax) context.TargetNode
            );

        var updateClassDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDBUpdateOperationFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (ClassDeclarationSyntax) context.TargetNode
            );


        var compilationAndClasses = context.CompilationProvider
            .Combine(putClassDeclarations.Collect())
            .Combine(updateClassDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, Execute);
    }

    // https://github.com/dotnet/runtime/blob/4ea93a6be4ea1b084158cf2aed7cac2414f10a2d/src/libraries/System.Text.Json/gen/JsonSourceGenerator.Roslyn4.0.cs
    private static void Execute(SourceProductionContext context,
        ((Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) Left, ImmutableArray<ClassDeclarationSyntax>
            Right) tuple)
    {
        var ((compilation, putClasses), updateClasses) = tuple;


        if (putClasses.IsDefaultOrEmpty && updateClasses.IsDefaultOrEmpty)
            return;

        var typeSymbols = compilation.GetTypeSymbols(putClasses.Concat(updateClasses).GetUnique());
        foreach (var typeSymbol in typeSymbols)
        {
            var repository = typeSymbol.CreateRepository(compilation);
            var code = typeSymbol.CreateNamespace(typeSymbol.CreateClass(repository));
            context.AddSource(typeSymbol.Name, code);
        }
    }

}