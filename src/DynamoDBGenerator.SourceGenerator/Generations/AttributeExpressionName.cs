using System.Collections.Immutable;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class AttributeExpressionName
{

    private const string ConstructorAttributeName = "nameRef";
    private const string ConstructorSetName = "set";
    private const string SetFieldName = "___set___";

    private static readonly Func<ITypeSymbol, string> TypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Names", SymbolEqualityComparer.Default);
    internal static IEnumerable<string> CreateClasses(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, ImmutableArray<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return arguments
            .SelectMany(x => CodeFactory.Create(x.EntityTypeSymbol, y => CreateStruct(y, getDynamoDbProperties, options), hashSet));

    }
    private static IEnumerable<string> TypeContent(
        ITypeSymbol typeSymbol,
        (bool IsUnknown, DynamoDbDataMember DDB, string IfBranchAlias, string DbRef, string NameRef, string AttributeReference, string AttributeInterfaceName)[] dataMembers,
        string structName)
    {
        const string self = "_self";
        var constructorFieldAssignments = dataMembers
            .Select(x =>
            {
                var ternaryExpressionName = $"{ConstructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{ConstructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                return x.IsUnknown
                    ? $"{x.NameRef} = new (() => new {x.AttributeReference}({ternaryExpressionName}, {ConstructorSetName}));"
                    : $"{x.NameRef} = new (() => {ternaryExpressionName});";
            })
            .Append($"{SetFieldName} = {ConstructorSetName};")
            .Append($@"{self} = new(() => {ConstructorAttributeName} ?? throw new NotImplementedException(""Root element AttributeExpressionName reference.""));");

        foreach (var fieldAssignment in $"public {structName}(string? {ConstructorAttributeName}, HashSet<KeyValuePair<string, string>> {ConstructorSetName})".CreateScope(constructorFieldAssignments))
            yield return fieldAssignment;

        foreach (var fieldDeclaration in dataMembers)
        {
            if (fieldDeclaration.IsUnknown)
            {
                yield return $"private readonly Lazy<{fieldDeclaration.AttributeReference}> {fieldDeclaration.NameRef};";
                yield return $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.NameRef}.Value;";
            }
            else
            {
                yield return $"private readonly Lazy<string> {fieldDeclaration.NameRef};";
                yield return $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.NameRef}.Value;";

            }
        }
        yield return $"private readonly Lazy<string> {self};";
        yield return $"private readonly HashSet<KeyValuePair<string, string>> {SetFieldName};";

        var yields = dataMembers
            .Select(static x => x.IsUnknown
                ? $@"if ({x.NameRef}.IsValueCreated) {{ if (new KeyValuePair<string, string>(""{x.DbRef}"", ""{x.DDB.AttributeName}"") is var {x.IfBranchAlias} && {SetFieldName}.Add({x.IfBranchAlias})) {{ yield return {x.IfBranchAlias}; }} foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionNameTrackerInterfaceAccessedNames}()) {{ yield return x; }} }}"
                : $@"if ({x.NameRef}.IsValueCreated && new KeyValuePair<string, string>(""{x.DbRef}"", ""{x.DDB.AttributeName}"") is var {x.IfBranchAlias} && {SetFieldName}.Add({x.IfBranchAlias})) yield return {x.IfBranchAlias};"
            )
            .Append($@"if ({self}.IsValueCreated) yield return new ({self}.Value, ""{typeSymbol.Name}"");");

        foreach (var s in $"IEnumerable<KeyValuePair<string, string>> {AttributeExpressionNameTrackerInterface}.{AttributeExpressionNameTrackerInterfaceAccessedNames}()".CreateScope(yields))
            yield return s;

        yield return $"public override string ToString() => {self}.Value;";
    }
    private static CodeFactory CreateStruct(ITypeSymbol typeSymbol, Func<ITypeSymbol, ImmutableArray<DynamoDbDataMember>> fn, MarshallerOptions options)
    {
        var dataMembers = fn(typeSymbol)
            .Select(x => (
                IsUnknown: !options.IsConvertable(x.DataMember.Type) && x.DataMember.Type.TypeIdentifier() is UnknownType,
                DDB: x,
                IfBranchAlias : $"__{x.DataMember.Name}__",
                DbRef: $"#{x.AttributeName}",
                NameRef: $"_{x.DataMember.Name}NameRef",
                AttributeReference: TypeName(x.DataMember.Type),
                AttributeInterfaceName: AttributeExpressionNameTrackerInterface
            ))
            .ToArray();

        var structName = TypeName(typeSymbol);

        var @class = $"public readonly struct {structName} : {AttributeExpressionNameTrackerInterface}".CreateScope(TypeContent(typeSymbol, dataMembers, structName));
        return new CodeFactory(@class, dataMembers.Where(x => x.IsUnknown).Select(x => x.DDB.DataMember.Type));

    }

    internal static (string method, string typeName) RootSignature(ITypeSymbol typeSymbol)
    {
        var typeName = TypeName(typeSymbol);
        return ($"public {typeName} {AttributeExpressionNameTrackerMethodName}() => new {typeName}(null, new HashSet<KeyValuePair<string ,string>>());", typeName);
    }
}
