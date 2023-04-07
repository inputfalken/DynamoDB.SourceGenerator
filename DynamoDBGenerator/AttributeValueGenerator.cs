using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DynamoDBGenerator
{
    [Generator]
    public class AttributeValueGenerator : IIncrementalGenerator
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

                var generateCode = GenerateCode(type);

                context.AddSource($"{typeNamespace}{type.Name}.g.cs", FormatCode(generateCode));
            }
        }

        private static string FormatCode(string codeToFormat)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(codeToFormat);
            var root = syntaxTree.GetRoot();
            var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace);

            using var writer = new StringWriter();
            formattedRoot.WriteTo(writer);
            return writer.ToString();
        }

        private static string CreateAttributeValue(ITypeSymbol typeSymbol, string propertyName)
        {
            static string ConstructList(string propertyName, ITypeSymbol elementType)
            {
                var linqAttributeValueSelection = $"Select(x => {CreateAttributeValue(elementType, "x")}))";

                return elementType.Name is nameof(Nullable)
                    ? $"L = new List<AttributeValue>({propertyName}.Where(x => x.HasValue).{linqAttributeValueSelection}"
                    : $"L = new List<AttributeValue>({propertyName}.{linqAttributeValueSelection}";
            }

            static string CreateAssignment(ITypeSymbol typeSymbol, string propertyName)
            {
                return typeSymbol switch
                {
                    {SpecialType: SpecialType.System_String} => $"S = {propertyName}",
                    {SpecialType: SpecialType.System_Boolean} => $"BOOL = {propertyName}",
                    {SpecialType: SpecialType.System_Int16 or SpecialType.System_Int32 or SpecialType.System_Int64} =>
                        $"N = {propertyName}.ToString()",
                    {SpecialType: SpecialType.System_DateTime} => $@"N = {propertyName}.ToString(""O"")",
                    not null when HasAttributeValueGeneratorAttribute(typeSymbol) =>
                        $"M = {propertyName}.BuildAttributeValues()",
                    INamedTypeSymbol
                    {
                        IsGenericType: true, TypeArguments.Length: 1
                    } singleGenericType => singleGenericType switch
                    {
                        not null when singleGenericType.Name == nameof(Nullable) =>
                            $"{CreateAssignment(singleGenericType.TypeArguments[0], $"{propertyName}.Value")}",
                        // Is IEnumerable implementation
                        not null when singleGenericType.AllInterfaces.Any(x => x.Name == nameof(IEnumerable)) =>
                            ConstructList(propertyName, singleGenericType.TypeArguments[0]),
                        _ => throw new NotSupportedException(
                            $"Could not generate AttributeValue for single generic type '{singleGenericType};")
                    },
                    IArrayTypeSymbol {ElementType: { } elementType} => ConstructList(propertyName, elementType),
                    _ => throw new NotSupportedException($"Could not generate AttributeValue for '{typeSymbol};")
                };
            }

            return @$"new AttributeValue {{ {CreateAssignment(typeSymbol, propertyName)} }}";
        }


        private static string BuildAttributeDictionaryMethod(string methodName, ITypeSymbol type)
        {
            const string dictionaryName = $"attributeValues";
            var assignments = GetDynamoDbProperties(type).Select(x =>
                {
                    var add = @$"{dictionaryName}.Add(""{x.Name}"", {CreateAttributeValue(x.Type, x.Name)});";
                    return x.Type.IsReferenceType || x.Type.Name == nameof(Nullable)
                        ? @$"if ({x.Name} != default) {{ {add} }} "
                        : add;
                })
                .ToArray();

            return @$"
public Dictionary<string, AttributeValue> {methodName}()
  {{ 
      var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: {assignments.Length});
      {string.Join(Environment.NewLine, assignments)}

       return {dictionaryName};
  }}";
        }

        private static string GenerateCode(ITypeSymbol type)
        {
            var nameSpace = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();
            var name = type.Name;
            // TODO add class with consts called 'AttribueValueKeys'

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

        private static IPropertySymbol? TryGetHashKey(INamespaceOrTypeSymbol type)
        {
            return type
                .GetPublicInstanceProperties()
                .SingleOrDefault(x =>
                    x.GetAttributes().Any(y => y.AttributeClass is {Name: nameof(DynamoDBHashKeyAttribute)}));
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
                nameof(AttributeValueGeneratorAttribute) => true,
                nameof(AttributeValueGenerator) => true,
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

            if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classDeclaration) is not ITypeSymbol type)
                return null;

            return HasAttributeValueGeneratorAttribute(type) is false ? null : type;
        }

        private static bool HasAttributeValueGeneratorAttribute(ISymbol type)
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