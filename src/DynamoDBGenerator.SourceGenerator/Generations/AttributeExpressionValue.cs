using System.Collections.Immutable;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class AttributeExpressionValue
{
    private static readonly Func<ITypeSymbol, string> TypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", SymbolEqualityComparer.Default);

    private const string ValueProvider = "valueIdProvider";
    private static IEnumerable<string> TypeContents(
        ITypeSymbol typeSymbol,
        (bool IsUnknown, DynamoDbDataMember DDB, string ValueRef, string AttributeReference, string AttributeInterfaceName)[] dataMembers,
        string structName,
        string interfaceName,
        MarshallerOptions options
    )
    {
        const string self = "_self";
        var constructorFieldAssignments = dataMembers
            .Select(x => x.IsUnknown
                ? $"_{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ValueProvider}, {MarshallerOptions.ParamReference}));"
                : $"{x.ValueRef} = new ({ValueProvider});")
            .Append($"{self} = new({ValueProvider});")
            .Append($"{MarshallerOptions.FieldReference} = {MarshallerOptions.ParamReference};");
        foreach (var fieldAssignment in $"public {structName}(Func<string> {ValueProvider}, {MarshallerOptions.Name} options)".CreateScope(constructorFieldAssignments))
            yield return fieldAssignment;

        yield return MarshallerOptions.FieldDeclaration;
        foreach (var fieldDeclaration in dataMembers)
        {
            if (fieldDeclaration.IsUnknown)
            {
                yield return $"private readonly Lazy<{fieldDeclaration.AttributeReference}> _{fieldDeclaration.DDB.DataMember.Name};";
                yield return $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => _{fieldDeclaration.DDB.DataMember.Name}.Value;";
            }
            else
            {
                yield return $"private readonly Lazy<string> {fieldDeclaration.ValueRef};";
                yield return $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.ValueRef}.Value;";
            }
        }
        yield return $"private readonly Lazy<string> {self};";

        const string param = "entity";

        var enumerable = Enumerable.Empty<string>();
        if (typeSymbol.IsNullable())
        {
            enumerable = $"if ({param} is null)".CreateScope($"yield return new ({self}.Value, {AttributeValueUtilityFactory.Null});", "yield break;");
        }
        else if (typeSymbol.IsReferenceType)
        {
            enumerable = $"if ({param} is null)".CreateScope($"throw {ExceptionHelper.NullExceptionMethod}(\"{structName}\");");
        }

        var yields = enumerable.Concat(
            dataMembers
                .Select(x =>
                    {
                        var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                        return x.IsUnknown
                            ? $"if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({accessPattern})) {{ yield return x; }}")}"
                            : $"if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {Marshaller.InvokeMarshallerMethod(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}", $"\"{x.DDB.DataMember.Name}\"", options, MarshallerOptions.FieldReference)} ?? {AttributeValueUtilityFactory.Null});")}";
                    }
                )
                .Append($"if ({self}.IsValueCreated) yield return new ({self}.Value, {Marshaller.InvokeMarshallerMethod(typeSymbol, "entity", $"\"{structName}\"", options, MarshallerOptions.FieldReference)} ?? {AttributeValueUtilityFactory.Null});")
        );

        foreach (var yield in
                 $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({typeSymbol.Representation().annotated} entity)"
                     .CreateScope(yields))
            yield return yield;

        yield return $"public override string ToString() => {self}.Value;";
    }
    internal static IEnumerable<string> CreateExpressionAttributeValue(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, ImmutableArray<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return arguments
            .SelectMany(x => CodeFactory.Create(x.ArgumentType, y => CreateStruct(y, getDynamoDbProperties, options), hashSet));
    }
    private static CodeFactory CreateStruct(ITypeSymbol typeSymbol, Func<ITypeSymbol, ImmutableArray<DynamoDbDataMember>> fn, MarshallerOptions options)
    {
        var dataMembers =
            options.IsConvertable(typeSymbol)
                ? Array
                    .Empty<(bool IsUnknown, DynamoDbDataMember DDB, string ValueRef, string AttributeReference, string
                        AttributeInterfaceName)>()
                : fn(typeSymbol)
                    .Select(x =>
                    {
                        return (
                            IsUnknown: !options.IsConvertable(x.DataMember.Type) &&
                                       x.DataMember.Type.TypeIdentifier() is UnknownType,
                            DDB: x,
                            ValueRef: $"_{x.DataMember.Name}ValueRef",
                            AttributeReference: TypeName(x.DataMember.Type),
                            AttributeInterfaceName:
                            $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{x.DataMember.Type.Representation().annotated}>"
                        );
                    })
                    .ToArray();

        var structName = TypeName(typeSymbol);
        var interfaceName = $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{typeSymbol.Representation().annotated}>";

        var @struct = $"public readonly struct {structName} : {interfaceName}".CreateScope(TypeContents(typeSymbol, dataMembers, structName, interfaceName, options));

        return new CodeFactory(@struct, dataMembers.Where(x => x.IsUnknown).Select(x => x.DDB.DataMember.Type));

    }

    internal static (IEnumerable<string> method, string typeName) RootSignature(ITypeSymbol typeSymbol)
    {
        var typeName = TypeName(typeSymbol);
        return ($"public {typeName} {Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerMethodName}()".CreateScope(
            "var incrementer = new DynamoExpressionValueIncrementer();",
            $"return new {typeName}(incrementer.GetNext, {MarshallerOptions.FieldReference});"
        ), typeName);
    }
}