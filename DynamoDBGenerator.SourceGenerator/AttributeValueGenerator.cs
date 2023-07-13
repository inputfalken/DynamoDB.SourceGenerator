using System.Collections.Immutable;
using System.Text;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
public class AttributeValueGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Constants.DynamoDbDocumentFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (ClassDeclarationSyntax)context.TargetNode
            );

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations,
        SourceProductionContext context
    )
    {
        foreach (var syntax in classDeclarations)
        {
            var symbol = compilation
                .GetSemanticModel(syntax.SyntaxTree)
                .GetDeclaredSymbol(syntax);

            if (symbol is null)
                continue;

            var type = (ITypeSymbol)symbol;

            var code = type.CreateNamespace(
                type.CreateClass(
                    type.GeneratePocoToAttributeValueFactory()
                )
            );

            context.AddSource(
                $"{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                SourceText.From(code, Encoding.UTF8)
            );
        }
    }
}