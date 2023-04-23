using System.Collections.Immutable;
using System.Text;
using DynamoDBGenerator.SourceGenerator.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DynamoDBGenerator.SourceGenerator
{
    [Generator]
    public class AttributeKeyGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var putRequestTypes = context.SyntaxProvider
                .CreateSyntaxProvider(IsAttributeValueAttribute, GetEnumTypeOrNull)
                .Where(x => x is not null)
                .Collect();

            context.RegisterSourceOutput(putRequestTypes, GenerateCode);
        }

        private static void GenerateCode(SourceProductionContext context, ImmutableArray<ITypeSymbol?> typeSymbols)
        {
            if (typeSymbols.IsDefaultOrEmpty)
                return;

            foreach (var type in typeSymbols)
            {
                if (type is null)
                    continue;

                var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
                    ? null
                    : $"{type.ContainingNamespace}.";


                var generateCode = type.CreateClassWithContent(BuildAttributeKeyClass);

                context.AddSource($"{typeNamespace}{nameof(AttributeKeyGenerator)}.{type.Name}.g.cs",
                    SourceText.From(generateCode, Encoding.UTF8));
            }
        }

        private static string BuildAttributeKeyClass(ITypeSymbol type)
        {
            var propertySymbols = type.GetDynamoDbProperties().ToArray();
            var constantDeclarations = propertySymbols
                .Select(x => @$"public const string {x.Name} = ""{x.Name}"";");
            var str = @$"public static class AttributeValueKeys
{{
    {string.Join(Constants.NewLine, constantDeclarations)}
    public static string[] Keys = new string[]{{{string.Join($",{Constants.NewLine}", propertySymbols.Select(x => x.Name))}}};
}}";

            return str;
        }


        private static bool IsAttributeValueAttribute(
            SyntaxNode syntaxNode,
            CancellationToken cancellationToken)
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
                nameof(AttributeKeyGenerator) => true,
                nameof(AttributeKeyGeneratorAttribute) => true,
                _ => false
            };
        }

        private static ITypeSymbol? GetEnumTypeOrNull(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            var attributeSyntax = (AttributeSyntax) context.Node;

            // "attribute.Parent" is "AttributeListSyntax"
            // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
            if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclaration)
                return null;

            if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol type)
                return null;

            return HasAttributeValueGeneratorAttribute(type) is false ? null : type;
        }

        private static bool HasAttributeValueGeneratorAttribute(ISymbol type)
        {
            return type
                .GetAttributes()
                .Any(a => a.AttributeClass is
                {
                    Name: nameof(AttributeKeyGeneratorAttribute),
                    ContainingNamespace:
                    {
                        Name: nameof(DynamoDBGenerator),
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                });
        }
    }
}