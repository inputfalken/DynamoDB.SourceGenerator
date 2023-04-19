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

        private static string CreateAttributeValue(ITypeSymbol typeSymbol, string accessPattern)
        {
            static string BuildList(ITypeSymbol elementType, string accessPattern)
            {
                var select = $"Select(x => {CreateAttributeValue(elementType, "x")}))";

                return IsNotGuaranteedToExist(elementType)
                    ? $"L = new List<AttributeValue>({accessPattern}.Where(x => x != default).{select}"
                    : $"L = new List<AttributeValue>({accessPattern}.{select}";
            }

            static string? BuildSet(ITypeSymbol elementType, string accessPattern)
            {
                if (IsNotGuaranteedToExist(elementType))
                    accessPattern = $"{accessPattern}.Where(x => x != default)";

                if (elementType.SpecialType is System_String)
                    return $"SS = new List<string>({accessPattern})";

                return IsNumeric(elementType) is false
                    ? null
                    : $"NS = new List<string>({accessPattern}.Select(x => x.ToString()))";
            }

            static bool IsNumeric(ITypeSymbol typeSymbol) => typeSymbol.SpecialType
                is System_Int16 or System_Byte
                or System_Int32 or System_Int64
                or System_SByte or System_UInt16
                or System_UInt32 or System_UInt64
                or System_Decimal or System_Double
                or System_Single;

            static string? SingleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
            {
                if (genericType is not INamedTypeSymbol type)
                    return null;

                if (type is not {IsGenericType: true, TypeArguments.Length: 1})
                    return null;

                var T = type.TypeArguments[0];

                return type switch
                {
                    {Name: nameof(Nullable)} => $"{CreateAssignment(T, $"{accessPattern}.Value")}",
                    _ when type.AllInterfaces.Any(x => x.Name is "ISet") => BuildSet(T, accessPattern),
                    _ when type.AllInterfaces.Any(x => x.Name is nameof(IEnumerable)) => BuildList(T, accessPattern),
                    _ => null
                };
            }

            static string? DoubleGenericTypeOrNull(ITypeSymbol genericType, string accessPattern)
            {
                if (genericType is not INamedTypeSymbol type)
                    return null;

                if (type is not {IsGenericType: true, TypeArguments.Length: 2})
                    return null;

                // ReSharper disable once InconsistentNaming
                var T1 = type.TypeArguments[0];
                // ReSharper disable once InconsistentNaming
                var T2 = type.TypeArguments[1];
                
                return type switch
                {
                    _ when T1.SpecialType is not System_String => null,
                    {Name: "Dictionary"} =>
                        $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {CreateAttributeValue(T2, "x.Value")})",
                    {Name: "KeyValuePair"} =>
                        $@"M = new Dictionary<string, AttributeValue>() {{ {{""{accessPattern}.Key"", {CreateAttributeValue(T2, $"{accessPattern}.Value")}}} }}",
                    _ => null
                };
            }


            static string? TimeStampOrNull(ITypeSymbol symbol, string accessPattern)
            {
                return symbol is {SpecialType: System_DateTime} or {Name: nameof(DateTimeOffset) or "DateOnly"}
                    ? $@"S = {accessPattern}.ToString(""O"")"
                    : null;
            }

            static string CreateAssignment(ITypeSymbol typeSymbol, string accessPattern)
            {
                return typeSymbol switch
                {
                    {SpecialType: System_String} => $"S = {accessPattern}",
                    {SpecialType: System_Boolean} => $"BOOL = {accessPattern}",
                    _ when IsNumeric(typeSymbol) => $"N = {accessPattern}.ToString()",
                    _ when TimeStampOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                    _ when IsAttributeValueGenerator(typeSymbol) => $"M = {accessPattern}.BuildAttributeValues()",
                    IArrayTypeSymbol {ElementType: { } elementType} => BuildList(elementType, accessPattern),
                    _ when SingleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                    _ when DoubleGenericTypeOrNull(typeSymbol, accessPattern) is { } assignment => assignment,
                    _ => throw new NotSupportedException($"Could not generate AttributeValue for '{typeSymbol}'.")
                };
            }

            return @$"new AttributeValue {{ {CreateAssignment(typeSymbol, accessPattern)} }}";
        }

        private static bool IsNotGuaranteedToExist(ITypeSymbol y)
        {
            return y.IsReferenceType || y.Name == nameof(Nullable);
        }

        private static string BuildAttributeDictionaryMethod(string methodName, ITypeSymbol type)
        {
            static string InitializeDictionary(string dictionaryName, IEnumerable<IPropertySymbol> propertySymbols)
            {
                var capacityAggregator = propertySymbols
                    .Aggregate(
                        (dynamicCount: new Queue<string>(), Count: 0),
                        (x, y) =>
                        {
                            if (IsNotGuaranteedToExist(y.Type) is false) return (x.dynamicCount, x.Count + 1);
                            x.dynamicCount.Enqueue($"(({y.Name} != default) ? (1) : (0))");
                            return (x.dynamicCount, x.Count);
                        }
                    );

                const string capacityName = "elementCount";
                const string dynamicCapacityName = "nonNullElementCount";

                var capacityCalculation = capacityAggregator.Count > 0 && capacityAggregator.dynamicCount.Count > 0
                    ? $"{capacityName} + {dynamicCapacityName}"
                    : capacityAggregator.Count > 0
                        ? capacityName
                        : capacityAggregator.dynamicCount.Count > 0
                            ? dynamicCapacityName
                            : null;

                var capacityVariable = capacityAggregator.Count > 0
                    ? $"const int {capacityName} = {capacityAggregator.Count};"
                    : null;
                var dynamicCapacityVariable = capacityAggregator.dynamicCount.Count > 0
                    ? $"var {dynamicCapacityName} = {string.Join(" + ", capacityAggregator.dynamicCount)};"
                    : null;


                return @$"
    {capacityVariable}
    {dynamicCapacityVariable}
    var {dictionaryName} = new Dictionary<string, AttributeValue>({capacityCalculation});
";
            }

            const string dictionaryName = "attributeValues";
            const string indent = "    ";
            var dynamoDbProperties = GetDynamoDbProperties(type).ToArray();

            var assignments = dynamoDbProperties.Select(x =>
                {
                    var add = @$"{dictionaryName}.Add(""{x.Name}"", {CreateAttributeValue(x.Type, x.Name)});";
                    return IsNotGuaranteedToExist(x.Type)
                        ? @$"if ({x.Name} != default) {{ {add} }} "
                        : add;
                })
                .Select(x => $"{indent}{x}");

            return @$"
public Dictionary<string, AttributeValue> {methodName}()
{{ 
{InitializeDictionary(dictionaryName, dynamoDbProperties)}
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