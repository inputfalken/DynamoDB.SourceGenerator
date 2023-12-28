using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class AttributeExpressionValue
{
    private static readonly Func<ITypeSymbol, string> TypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", SymbolEqualityComparer.Default);

    private const string ValueProvider = "valueIdProvider";
    private static IEnumerable<string> CreateCode(
        ITypeSymbol typeSymbol,
        (bool IsUnknown, DynamoDbDataMember DDB, string ValueRef, string AttributeReference, string AttributeInterfaceName, TypeIdentifier typeIdentifier)[] dataMembers,
        string structName,
        string interfaceName,
        MarshallerOptions options
    )
    {
        const string self = "_self";
        var constructorFieldAssignments = dataMembers
            .Select(x => x.IsUnknown
                ? $"_{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ValueProvider}, options));"
                : $"{x.ValueRef} = new ({ValueProvider});")
            .Append($"{self} = new({ValueProvider});")
            .Append($"{MarshallerOptions.PropertyName} = options;");
        foreach (var fieldAssignment in $"public {structName}(Func<string> {ValueProvider}, {MarshallerOptions.Name} options)".CreateBlock(constructorFieldAssignments))
            yield return fieldAssignment;

        yield return $"private readonly {MarshallerOptions.Name} {MarshallerOptions.PropertyName};";
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
            enumerable = $"if ({param} is null)".CreateBlock($"yield return new ({self}.Value, {AttributeValueUtilityFactory.Null});", "yield break;");
        }
        else if (typeSymbol.IsReferenceType)
        {
            enumerable = $"if ({param} is null)".CreateBlock($"throw {ExceptionHelper.NullExceptionMethod}(\"{structName}\");");
        }

        var yields = enumerable.Concat(
            dataMembers
                .Select(x =>
                    {
                        var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                        return x.IsUnknown
                            ? $"if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({accessPattern})) {{ yield return x; }}")}"
                            : $"if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {Marshaller.InvokeMarshallerMethod(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}", $"\"{x.DDB.DataMember.Name}\"", options)} ?? {AttributeValueUtilityFactory.Null});")}";
                    }
                )
                .Append($"if ({self}.IsValueCreated) yield return new ({self}.Value, {Marshaller.InvokeMarshallerMethod(typeSymbol, "entity", $"\"{structName}\"", options)} ?? {AttributeValueUtilityFactory.Null});")
        );

        foreach (var yield in
                 $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({typeSymbol.Representation().annotated} entity)"
                     .CreateBlock(yields))
            yield return yield;

        yield return $"public override string ToString() => {self}.Value;";
    }
    internal static IEnumerable<string> CreateExpressionAttributeValue(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return arguments
            .SelectMany(x => Conversion.ConversionMethods(x.ArgumentType, y => CreateStruct(y, getDynamoDbProperties, options), hashSet)).SelectMany(x => x.Code);
    }
    private static Conversion CreateStruct(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn, MarshallerOptions options)
    {
        var dataMembers = fn(typeSymbol)
            .Select(x =>
            {
                var typeIdentifier = x.DataMember.Type.TypeIdentifier();
                var valueRef = $"_{x.DataMember.Name}ValueRef";
                var attributeReference = TypeName(x.DataMember.Type);
                var isUnknown = typeIdentifier is UnknownType;

                return (
                    IsUnknown: isUnknown,
                    DDB: x,
                    ValueRef: valueRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{x.DataMember.Type.Representation().annotated}>",
                    typeIdentifier
                );
            })
            .ToArray();

        var structName = TypeName(typeSymbol);
        var interfaceName = $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{typeSymbol.Representation().annotated}>";

        var @struct = $"public readonly struct {structName} : {interfaceName}".CreateBlock(CreateCode(typeSymbol, dataMembers, structName, interfaceName, options));

        return new Conversion(@struct, dataMembers.Select(x => x.typeIdentifier).OfType<UnknownType>().Select(x => x.TypeSymbol));

    }

    internal static (IEnumerable<string> method, string typeName) RootSignature(ITypeSymbol typeSymbol)
    {
        var typeName = TypeName(typeSymbol);
        return ($"public {typeName} {Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerMethodName}()".CreateBlock(
            "var incrementer = new DynamoExpressionValueIncrementer();",
            $"return new {typeName}(incrementer.GetNext, {MarshallerOptions.PropertyName});"
        ), typeName);
    }
}