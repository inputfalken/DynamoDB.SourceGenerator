using System.Collections.Immutable;
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

            var dictionaryMethod = type.GetDynamoDbProperties()
                .CreateStaticAttributeValueDictionaryMethod(type, Constants.AttributeValueGeneratorMethodName);

            var dictionaries = dictionaryMethod.Mappings
                .Where(x => x.AttributeValue.How == AttributeValueInstance.Decision.NeedsExternalInvocation)
                .Select(x => x.Item2.DataMember.Type)
                .Distinct(SymbolEqualityComparer.Default) // Is needed in order to make sure we dont create multiple dictionaries for the same type.
                .Cast<ITypeSymbol>()
                // Might need to look at the results from `CreateStaticAttributeValueDictionaryMethod` to verify whether we support the type.
                .Select(x => x.GetDynamoDbProperties().CreateStaticAttributeValueDictionaryMethod(x, Constants.AttributeValueGeneratorMethodName))
                .Prepend(dictionaryMethod)
                .Select(x => x.Code)
                .Prepend(AttributeValueCodeGenerationExtensions.CreateAttributeValueDictionaryRootMethod(Constants.AttributeValueGeneratorMethodName));
            

            // TODO In order to map nested classes & types that are not marked with AttributeValueGeneratorAttribute:
            // * Make all dictionary methods private static with a parameter that is the type to be mapped.
            // * Only have one instance method with AttributeValueGeneratorMethodName that will invoke the static methods.
            // * In order to find nested classes you need to do type.GetTypeMembers().Where(x => x.TypeKind is TypeKind.Class);
            var code = type.CreateNamespace(type.CreateClass(string.Join(Constants.NewLine, dictionaries)));

            context.AddSource(
                $"{typeNamespace}{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                SourceText.From(code, Encoding.UTF8)
            );
        }
    }
}