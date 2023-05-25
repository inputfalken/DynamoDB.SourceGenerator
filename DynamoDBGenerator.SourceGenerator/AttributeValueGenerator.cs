using System.Collections.Immutable;
using System.Text;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;
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
                Constants.AttributeValueGeneratorFullName,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (ClassDeclarationSyntax) context.TargetNode
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

            var type = (ITypeSymbol) symbol;

            var attributeData = type.GetAttributes().First(a => a.AttributeClass is
            {
                Name: nameof(AttributeValueGeneratorAttribute),
                ContainingNamespace:
                {
                    Name: nameof(DynamoDBGenerator),
                    ContainingNamespace.IsGlobalNamespace: true
                }
            });

            var settings = attributeData.CreateInstance<AttributeValueGeneratorAttribute>()!;

            var code = type.CreateNamespace(
                type.CreateClass(
                    type.GeneratePocoToAttributeValueFactory(
                        new Settings
                        {
                            ConsumerMethodConfig = new Settings.ConsumerMethodConfiguration(settings.MethodName)
                        }
                    ).Code
                )
            );

            context.AddSource(
                $"{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                SourceText.From(code, Encoding.UTF8)
            );
        }
    }
}