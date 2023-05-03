using System.Collections.Immutable;
using System.Text;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration;
using DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.AttributeValue;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DynamoDBGenerator.SourceGenerator;

[Generator]
public class AttributeValueGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributeValueGenerators = context.SyntaxProvider
            .CreateSyntaxProvider((syntaxNode, _) =>
                {
                    if (syntaxNode is not AttributeSyntax attribute)
                        return false;

                    var name = attribute.Name switch
                    {
                        SimpleNameSyntax ins => ins.Identifier.Text,
                        QualifiedNameSyntax qns => qns.Right.Identifier.Text,
                        _ => null
                    };

                    return name switch
                    {
                        "AttributeValueGeneratorAttribute" => true,
                        "AttributeValueGenerator" => true,
                        _ => false
                    };
                },
                (ctx, _) =>
                {
                    var attributeSyntax = (AttributeSyntax) ctx.Node;

                    // "attribute.Parent" is "AttributeListSyntax"
                    // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
                    if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
                        return null;

                    if (ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol type)
                        return null;

                    var attribute = type
                        .GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass is
                        {
                            Name: nameof(AttributeValueGeneratorAttribute),
                            ContainingNamespace:
                            {
                                Name: nameof(DynamoDBGenerator),
                                ContainingNamespace.IsGlobalNamespace: true
                            }
                        });

                    if (attribute is null) return ((ITypeSymbol, AttributeValueGeneratorAttribute)?) null;

                    return (type, attribute.CreateInstance<AttributeValueGeneratorAttribute>()!);
                }
            )
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(attributeValueGenerators, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context,
        ImmutableArray<(ITypeSymbol, AttributeValueGeneratorAttribute)?> typeSymbols)
    {
        const string mPropertyMethodName = nameof(AttributeValueGeneratorAttribute)
                                           + "_"
                                           + nameof(DynamoDBGenerator)
                                           + "_"
                                           + nameof(SourceGenerator);
        foreach (var tuple in typeSymbols)
        {
            if (tuple is null)
                continue;

            var type = tuple.Value.Item1;
            var settings = tuple.Value.Item2;

            var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{type.ContainingNamespace}.";

            var code = type.CreateNamespace(
                type.CreateClass(
                    type.CreateAttributeConversionCode(
                        new AttributeValueConversionSettings(mPropertyMethodName),
                        settings.MethodName
                    )
                )
            );
            context.AddSource(
                $"{typeNamespace}{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                SourceText.From(code, Encoding.UTF8)
            );
        }
    }
}