﻿using System.Collections.Immutable;
using System.Text;
using DynamoDBGenerator.SourceGenerator.Extensions;
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

                    return type.IsAttributeValueGenerator() is false ? null : type;
                }
            )
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(attributeValueGenerators, GenerateCode!);
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol> typeSymbols)
    {
        foreach (var type in typeSymbols)
        {
            if (type is null)
                continue;

            var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : $"{type.ContainingNamespace}.";

            var generateCode = type.CreateClassWithContent(x =>
                x.GetDynamoDbProperties()
                    .CreateAttributeValueDictionaryMethod(Constants.AttributeValueGeneratorMethodName)
            );

            context.AddSource(
                $"{typeNamespace}{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                SourceText.From(generateCode, Encoding.UTF8)
            );
        }
    }
}