using System.Collections;
using System.Collections.Immutable;
using System.Text;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.SpecialType;

namespace DynamoDBGenerator.SourceGenerator
{
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
                            nameof(AttributeValueGeneratorAttribute) => true,
                            nameof(AttributeValueGenerator) => true,
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

                        return IsAttributeValueGenerator(type) is false ? null : type;
                    }
                )
                .Where(x => x is not null)
                .Collect();

            context.RegisterSourceOutput(attributeValueGenerators, GenerateCode);
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

                var generateCode = GenerateCode(type);

                context.AddSource(
                    $"{typeNamespace}{nameof(AttributeValueGenerator)}.{type.Name}.g.cs",
                    SourceText.From(generateCode, Encoding.UTF8)
                );
            }
        }

        private static string CreateAttributeValue(ITypeSymbol typeSymbol, string propertyName)
        {
            static string BuildList(string propertyName, ITypeSymbol elementType)
            {
                var select = $"Select(x => {CreateAttributeValue(elementType, "x")}))";

                return elementType.Name is nameof(Nullable)
                    ? $"L = new List<AttributeValue>({propertyName}.Where(x => x.HasValue).{select}"
                    : $"L = new List<AttributeValue>({propertyName}.{select}";
            }

            static string? BuildSet(string propertyName, ITypeSymbol elementType)
            {
                if (elementType.SpecialType is System_String)
                    return $"SS = new List<string>({propertyName})";

                return IsNumeric(elementType) is false
                    ? null
                    : $"NS = new List<string>({propertyName}.Select(x => x.ToString()))";
            }

            static bool IsNumeric(ITypeSymbol typeSymbol) => typeSymbol.SpecialType
                is System_Int16 or System_Byte
                or System_Int32 or System_Int64
                or System_SByte or System_UInt16
                or System_UInt32 or System_UInt64
                or System_Decimal or System_Double
                or System_Single;

            static string? SingleGenericTypeOrNull(ITypeSymbol genericType, string propAccess)
            {
                if (genericType is not INamedTypeSymbol type)
                    return null;

                if (type is not {IsGenericType: true, TypeArguments.Length: 1})
                    return null;

                var T = type.TypeArguments[0];

                return type switch
                {
                    {Name: nameof(Nullable)} => $"{CreateAssignment(T, $"{propAccess}.Value")}",
                    _ when type.AllInterfaces.Any(x => x.Name is "ISet") => BuildSet(propAccess, T),
                    _ when type.AllInterfaces.Any(x => x.Name is nameof(IEnumerable)) => BuildList(propAccess, T),
                    _ => null
                };
            }

            static string? TimeStampOrNull(ITypeSymbol symbol, string propertyAccessor)
            {
                return symbol is {SpecialType: System_DateTime} or {Name: nameof(DateTimeOffset) or "DateOnly"}
                    ? $@"S = {propertyAccessor}.ToString(""O"")"
                    : null;
            }

            static string CreateAssignment(ITypeSymbol typeSymbol, string propertyAccessor)
            {
                return typeSymbol switch
                {
                    {SpecialType: System_String} => $"S = {propertyAccessor}",
                    {SpecialType: System_Boolean} => $"BOOL = {propertyAccessor}",
                    _ when IsNumeric(typeSymbol) => $"N = {propertyAccessor}.ToString()",
                    _ when TimeStampOrNull(typeSymbol, propertyAccessor) is { } assignment => assignment,
                    _ when IsAttributeValueGenerator(typeSymbol) => $"M = {propertyAccessor}.BuildAttributeValues()",
                    IArrayTypeSymbol {ElementType: { } elementType} => BuildList(propertyAccessor, elementType),
                    _ when SingleGenericTypeOrNull(typeSymbol, propertyAccessor) is { } assignment => assignment,
                    _ => throw new NotSupportedException($"Could not generate AttributeValue for '{typeSymbol}'.")
                };
            }

            return @$"new AttributeValue {{ {CreateAssignment(typeSymbol, propertyName)} }}";
        }

        private static string BuildAttributeDictionaryMethod(string methodName, ITypeSymbol type)
        {
            const string dictionaryName = "attributeValues";
            const string indent = "    ";
            var assignments = GetDynamoDbProperties(type).Select(x =>
                {
                    var add = @$"{dictionaryName}.Add(""{x.Name}"", {CreateAttributeValue(x.Type, x.Name)});";
                    return x.Type.IsReferenceType || x.Type.Name == nameof(Nullable)
                        ? @$"if ({x.Name} != default) {{ {add} }} "
                        : add;
                })
                .Select(x => $"{indent}{x}")
                .ToArray();

            return @$"
public Dictionary<string, AttributeValue> {methodName}()
{{ 
    var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: {assignments.Length});
{string.Join(Constants.NewLine, assignments)}

    return {dictionaryName};
}}";
        }

        private static string GenerateCode(ITypeSymbol type)
        {
            var nameSpace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();
            var name = type.Name;

            return @$"// <auto-generated />

using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using System.Linq;

{(nameSpace is null ? null : $@"namespace {nameSpace}
{{")}
   partial class {name}
   {{
      {BuildAttributeDictionaryMethod("BuildAttributeValues", type)}
   }}
{(nameSpace is null ? null : @"}
")}";
        }

        private static IEnumerable<IPropertySymbol> GetDynamoDbProperties(INamespaceOrTypeSymbol type)
        {
            return type
                .GetPublicInstanceProperties()
                .Where(x => x.GetAttributes().Any(y => y.AttributeClass is {Name: nameof(DynamoDBPropertyAttribute)}));
        }


        private static bool IsAttributeValueGenerator(
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
                nameof(AttributeValueGeneratorAttribute) => true,
                nameof(AttributeValueGenerator) => true,
                _ => false
            };
        }

        private static bool IsAttributeValueGenerator(ISymbol type)
        {
            return type
                .GetAttributes()
                .Any(a => a.AttributeClass is
                {
                    Name: nameof(AttributeValueGeneratorAttribute),
                    ContainingNamespace:
                    {
                        Name: nameof(DynamoDBGenerator),
                        ContainingNamespace.IsGlobalNamespace: true
                    }
                });
        }
    }
}