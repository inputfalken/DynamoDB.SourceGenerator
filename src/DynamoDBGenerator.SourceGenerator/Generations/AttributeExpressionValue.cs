using System.Runtime.CompilerServices;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
using Marshaller = DynamoDBGenerator.SourceGenerator.Generations.Marshalling.Marshaller;

namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class AttributeExpressionValue
{
    private const string Suffix = "Values";

    private static readonly Func<ITypeSymbol, string> TypeName =
        TypeExtensions.SuffixedTypeSymbolNameFactory(Suffix, SymbolEqualityComparer.Default);

    private static readonly Func<ITypeSymbol, ITypeSymbol, string> GlobalTypeName =
        TypeExtensions.SuffixedFullyQualifiedTypeName(Suffix, SymbolEqualityComparer.Default);

    private const string ValueProvider = "valueIdProvider";

    private static IEnumerable<string> TypeContents(
        TypeIdentifier typeIdentifier,
        (bool IsUnknown, DynamoDbDataMember DDB, string AttributeReference, string AttributeInterfaceName)[]
            dataMembers,
        string structName,
        string interfaceName,
        MarshallerOptions options
    )
    {
        const string self = "_this";
        var constructorFieldAssignments = dataMembers
            .Select(x => x.IsUnknown
                ? $"{x.DDB.DataMember.NameAsPrivateField} = new (() => new ({ValueProvider}, {MarshallerOptions.ParamReference}));"
                : $"{x.DDB.DataMember.NameAsPrivateField} = new ({ValueProvider});")
            .Append($"{self} = new({ValueProvider});")
            .Append($"{MarshallerOptions.FieldReference} = {MarshallerOptions.ParamReference};");
        foreach (var fieldAssignment in $"public {structName}(Func<string> {ValueProvider}, {options.FullName} options)"
                     .CreateScope(constructorFieldAssignments))
            yield return fieldAssignment;

        yield return options.FieldDeclaration;
        foreach (var fieldDeclaration in dataMembers)
        {
            if (fieldDeclaration.IsUnknown)
            {
                yield return
                    $"private readonly Lazy<{fieldDeclaration.AttributeReference}> {fieldDeclaration.DDB.DataMember.NameAsPrivateField};";
                yield return
                    $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.DDB.DataMember.NameAsPrivateField}.Value;";
            }
            else
            {
                yield return $"private readonly Lazy<string> {fieldDeclaration.DDB.DataMember.NameAsPrivateField};";
                yield return
                    $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.DDB.DataMember.NameAsPrivateField}.Value;";
            }
        }

        yield return $"private readonly Lazy<string> {self};";

        const string param = "entity";

        var yields = (typeIdentifier switch
            {
                {IsNullable:true}  => $"if ({param} is null)".CreateScope(
                    $"yield return new ({self}.Value, {AttributeValueUtilityFactory.Null});", "yield break;"),
                { TypeSymbol.IsReferenceType: true } => $"if ({param} is null)".CreateScope(
                    $"throw {ExceptionHelper.NullExceptionMethod}(\"{structName}\");"),
                _ => Enumerable.Empty<string>()
            })
            .Concat(dataMembers
                .SelectMany(x => YieldSelector(x, options))
                .Concat(
                    $"if ({self}.IsValueCreated)"
                        .CreateScope(
                            $"yield return new ({self}.Value, {Marshaller.InvokeMarshallerMethod(typeIdentifier, "entity", $"\"{structName}\"", options, MarshallerOptions.FieldReference)}{HandeNullability(typeIdentifier)});"
                        )
                )
            )
            .ScopeTo(
                $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({typeIdentifier.AnnotatedString} entity)");


        foreach (var yield in yields)
            yield return yield;

        yield return $"public override string ToString() => {self}.Value;";
    }

    private static string? HandeNullability(TypeIdentifier typeSymbol) =>
        typeSymbol.IsNullable ? $" ?? {AttributeValueUtilityFactory.Null}" : null;

    private static IEnumerable<string> YieldSelector(
        (bool IsUnknown, DynamoDbDataMember DDB, string AttributeReference, string AttributeInterfaceName) x,
        MarshallerOptions options)
    {
        var accessPattern = $"entity.{x.DDB.DataMember.Name}";

        if (x.IsUnknown)
        {
            return x.DDB.DataMember.TypeIdentifier.TypeSymbol.NotNullIfStatement(
                    accessPattern,
                    $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerAccessedValues}({accessPattern}))"
                        .CreateScope("yield return x;")
                )
                .ScopeTo($"if ({x.DDB.DataMember.NameAsPrivateField}.IsValueCreated)");
        }


        return $"if ({x.DDB.DataMember.NameAsPrivateField}.IsValueCreated)".CreateScope(
            x.DDB.DataMember.TypeIdentifier.TypeSymbol.NotNullIfStatement(
                accessPattern,
                $"yield return new ({x.DDB.DataMember.NameAsPrivateField}.Value, {Marshaller.InvokeMarshallerMethod(x.DDB.DataMember.TypeIdentifier, $"entity.{x.DDB.DataMember.Name}", $"\"{x.DDB.DataMember.Name}\"", options, MarshallerOptions.FieldReference)}{HandeNullability(x.DDB.DataMember.TypeIdentifier)});"
            ));
    }

    internal static IEnumerable<string> CreateClasses(DynamoDBMarshallerArguments[] arguments,
        Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<TypeIdentifier>(TypeIdentifier.Default);

        return arguments
            .SelectMany(x =>
                CodeFactory.Create(x.ArgumentType, y => CreateStruct(y, getDynamoDbProperties, options), hashSet));
    }

    private static CodeFactory CreateStruct(TypeIdentifier typeIdentifier, Func<ITypeSymbol, DynamoDbDataMember[]> fn,
        MarshallerOptions options)
    {
        var dataMembers =
            options.IsConvertable(typeIdentifier.TypeSymbol)
                ? Array
                    .Empty<(bool IsUnknown, DynamoDbDataMember DDB, string AttributeReference, string
                        AttributeInterfaceName)>()
                : fn(typeIdentifier.TypeSymbol)
                    .Select(x => (
                        IsUnknown: options.IsUnknown(x.DataMember.TypeIdentifier),
                        DDB: x,
                        AttributeReference: TypeName(x.DataMember.TypeIdentifier.TypeSymbol),
                        AttributeInterfaceName:
                        $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{x.DataMember.TypeIdentifier.AnnotatedString}>"
                    ))
                    .ToArray();

        var structName = TypeName(typeIdentifier.TypeSymbol);
        var interfaceName =
            $"{Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerInterface}<{typeIdentifier.AnnotatedString}>";

        var @struct =
            $"public readonly struct {structName} : {interfaceName}".CreateScope(TypeContents(typeIdentifier, dataMembers,
                structName, interfaceName, options));

        return new CodeFactory(@struct, dataMembers.Where(x => x.IsUnknown).Select(x => x.DDB.DataMember.TypeIdentifier));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GloballyAccessibleName(ITypeSymbol parentClass, ITypeSymbol nestedClass) =>
        GlobalTypeName(parentClass, nestedClass);

    internal static (IEnumerable<string> method, string typeName) RootSignature(ITypeSymbol parentClass,
        ITypeSymbol nestedClass)
    {
        var typeName = GlobalTypeName(parentClass, nestedClass);
        return (
            $"public {typeName} {Constants.DynamoDBGenerator.Marshaller.AttributeExpressionValueTrackerMethodName}()"
                .CreateScope(
                    "var incrementer = new DynamoExpressionValueIncrementer();",
                    $"return new {typeName}(incrementer.GetNext, {MarshallerOptions.FieldReference});"
                ), typeName);
    }
}