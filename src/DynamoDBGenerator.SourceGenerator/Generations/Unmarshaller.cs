using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class Unmarshaller
{
    private const string DataMember = "dataMember";
    private const string Dict = "dict";
    private static readonly Func<ITypeSymbol, string> GetDeserializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("_U", SymbolEqualityComparer.IncludeNullability);
    private const string UnMarshallerClass = $"_{Constants.DynamoDBGenerator.Marshaller.UnmarshalMethodName}_";
    private const string Value = "attributeValue";
    private static IEnumerable<(bool useParentheses, IEnumerable<string> assignments)> Assignments(ITypeSymbol type, (DynamoDbDataMember DDB, string MethodCall, string Name)[] assignments)
    {
        const string indent = "    ";
        if (type.IsTupleType)
            yield return (true, assignments.Select(x => $"{indent}{x.DDB.DataMember.Name}: {x.MethodCall}"));
        else
        {
            var resolve = assignments
                .GroupJoin(
                    TryGetMatchedConstructorArguments(type),
                    x => x.DDB.DataMember.Name,
                    x => x.DataMember,
                    (x, y) => (x.MethodCall, x.DDB, Constructor: y.OfType<(string DataMember, string ParameterName)?>().FirstOrDefault())
                )
                .GroupBy(x => x.Constructor.HasValue)
                .OrderByDescending(x => x.Key)
                .Select(x =>
                {
                    return x.Key
                        ? (x.Key, x.Select(z => $"{indent}{z.Constructor!.Value.ParameterName} : {z.MethodCall}"))
                        : (x.Key, x.Where(z => z.DDB.DataMember.IsAssignable).Select(z => $"{indent}{z.DDB.DataMember.Name} = {z.MethodCall}"));
                });

            foreach (var valueTuple in resolve)
                yield return valueTuple;
        }

    }
    internal static IEnumerable<string> CreateClass(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        return $"private static class {UnMarshallerClass}".CreateBlock(CreateUnMarshaller(arguments, getDynamoDbProperties));
    }
    private static Conversion CreateCode(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        var assignments = fn(type)
            .Select(x => (DDB: x, MethodCall: InvokeUnmarshallMethod(x.DataMember.Type, $"{Dict}.GetValueOrDefault(\"{x.AttributeName}\")", $"\"{x.DataMember.Name}\""), x.DataMember.Name))
            .ToArray();

        var typeName = type.Representation();
        var blockBody =
            $"if ({Dict} is null)"
                .CreateBlock(type.IsNullable() ? "return null;" : $"throw {Constants.DynamoDBGenerator.ExceptionHelper.NullExceptionMethod}({DataMember});").Concat(
                    Assignments(type, assignments)
                        .AllAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
                        .SelectMany(x => x)
                        .DefaultIfEmpty("();")
                        // Is needed in order to not perform new entity? where '?' is not allowed in the end of the string.
                        .Prepend(type.IsTupleType ? "return" : $"return new {(typeName.annotated.EndsWith("?") ? typeName.annotated.Substring(0, typeName.annotated.Length - 1) : typeName.annotated)}")
                );

        var method = $"public static {typeName.annotated} {GetDeserializationMethodName(type)}(Dictionary<string, AttributeValue>? {Dict}, string? {DataMember} = null)".CreateBlock(blockBody);

        return new Conversion(method, assignments.Select(x => x.DDB.DataMember.Type));

    }
    private static Conversion CreateMethod(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {

        return type.TypeIdentifier() switch
        {
            BaseType baseType when CreateSignature(baseType) is var signature => baseType.Type switch
            {
                BaseType.SupportedType.String => signature
                    .CreateBlock($"return {Value} is {{ S: {{ }} x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Bool => signature
                    .CreateBlock($"return {Value} is {{ BOOL: var x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Char => signature
                    .CreateBlock($"return {Value} is {{ S: {{ }} x }} ? x[0] : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Enum => signature
                    .CreateBlock($"return {Value} is {{ N: {{ }} x }} ? ({baseType.TypeSymbol.Representation().annotated})Int32.Parse(x) : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Int16
                    or BaseType.SupportedType.Byte
                    or BaseType.SupportedType.Int32
                    or BaseType.SupportedType.Int64
                    or BaseType.SupportedType.SByte
                    or BaseType.SupportedType.UInt16
                    or BaseType.SupportedType.UInt32
                    or BaseType.SupportedType.UInt64
                    or BaseType.SupportedType.Decimal
                    or BaseType.SupportedType.Double
                    or BaseType.SupportedType.Single
                    => signature
                        .CreateBlock($"return {Value} is {{ N: {{ }} x }} ? {baseType.TypeSymbol.Representation().original}.Parse(x) : {Else(baseType)};")
                        .ToConversion(),
                BaseType.SupportedType.DateTime
                    or BaseType.SupportedType.DateTimeOffset
                    or BaseType.SupportedType.DateOnly
                    => signature
                        .CreateBlock($"return {Value} is {{ S: {{ }} x }} ? {baseType.TypeSymbol.Representation().original}.Parse(x) : {Else(baseType)};")
                        .ToConversion(),
                BaseType.SupportedType.MemoryStream => signature
                    .CreateBlock($"return {Value} is {{ B: {{ }} x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                _ => throw UncoveredConversionException(baseType, nameof(CreateMethod))
            },
            SingleGeneric singleGeneric when CreateSignature(singleGeneric) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateBlock($"return {Value} is null or {{ NULL: true }} ? {Else(singleGeneric)} : {InvokeUnmarshallMethod(singleGeneric.T, Value, DataMember)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.ICollection => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"")}).ToList() : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Array or SingleGeneric.SupportedType.IReadOnlyCollection => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"")}).ToArray() : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IEnumerable => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"")}) : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String => signature
                    .CreateBlock(
                        $"return {Value} is {{ SS : {{ }} x }} ? new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{(singleGeneric.T.IsNullable() ? "string?" : "string")}>" : null)}({(singleGeneric.T.IsNullable() ? "x" : $"x.Select((y,i) => y ?? throw {Constants.DynamoDBGenerator.ExceptionHelper.NullExceptionMethod}($\"{{{DataMember}}}[UNKNOWN]\")")})) : {Else(singleGeneric)};")
                    .ToConversion(),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric() => signature
                    .CreateBlock(
                        $"return {Value} is {{ NS : {{ }} x }} ? new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{singleGeneric.T.Representation().original}>" : null)}(x.Select(y => {singleGeneric.T.Representation().original}.Parse(y))) : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.TypeSymbol),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateBlock($"return {Value} is {{ M: {{ }} x }} ? x.ToDictionary(y => y.Key, y => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "y.Value", "y.Key")}) : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateBlock(
                        $"return {Value} is {{ M: {{ }} x }} ? x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "y.z", "y.Key")}) : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))

            },
            UnknownType => CreateCode(type, fn),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))
        };

    }

    private static string CreateSignature(TypeIdentifier typeIdentifier)
    {
        return $"public static {typeIdentifier.TypeSymbol.Representation().annotated} {GetDeserializationMethodName(typeIdentifier.TypeSymbol)}(AttributeValue? {Value}, string? {DataMember} = null)";
    }
    private static IEnumerable<string> CreateUnMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);
        return arguments.SelectMany(x =>
                Conversion.ConversionMethods(
                    x.EntityTypeSymbol,
                    y => CreateMethod(y, getDynamoDbProperties),
                    hashSet
                )
            )
            .SelectMany(x => x.Code);
    }
    private static string Else(TypeIdentifier typeIdentifier)
    {
        return typeIdentifier.TypeSymbol.IsNullable() ? "null" : $"throw {Constants.DynamoDBGenerator.ExceptionHelper.NullExceptionMethod}({DataMember})";
    }

    private static string InvokeUnmarshallMethod(ITypeSymbol typeSymbol, string paramReference, string dataMember)
    {
        return typeSymbol.TypeIdentifier() is UnknownType
            ? $"{GetDeserializationMethodName(typeSymbol)}({paramReference}?.M, {dataMember})"
            : $"{GetDeserializationMethodName(typeSymbol)}({paramReference}, {dataMember})";

    }
    private static IEnumerable<string> ObjectAssignmentBlock(bool useParentheses, IEnumerable<string> assignments, bool applySemiColon)
    {
        if (useParentheses)
        {
            yield return "(";

            foreach (var assignment in assignments.AllButLast(s => $"{s},"))
                yield return assignment;

            if (applySemiColon)
                yield return ");";
            else
                yield return ")";
        }
        else
        {
            yield return "{";

            foreach (var assignment in assignments.AllButLast(s => $"{s},"))
                yield return assignment;

            if (applySemiColon)
                yield return "};";
            else
                yield return "}";

        }

    }

    internal static IEnumerable<string> RootSignature(ITypeSymbol typeSymbol, string rootTypeName)
    {
        return $"public {rootTypeName} {Constants.DynamoDBGenerator.Marshaller.UnmarshalMethodName}(Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> entity)".CreateBlock(
            "ArgumentNullException.ThrowIfNull(entity);",
            $"return {UnMarshallerClass}.{GetDeserializationMethodName(typeSymbol)}(entity);");
    }
    private static IEnumerable<(string DataMember, string ParameterName)> TryGetMatchedConstructorArguments(ITypeSymbol typeSymbol)
    {

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return Enumerable.Empty<(string, string )>();

        if (namedTypeSymbol.InstanceConstructors.Length is 0)
            return Enumerable.Empty<(string, string )>();

        if (namedTypeSymbol is {Name: "KeyValuePair", ContainingNamespace.Name: nameof(System.Collections.Generic)})
            return namedTypeSymbol.InstanceConstructors
                .First(x => x.Parameters.Length is 2)
                .Parameters
                .Select(x => (MemberName: $"{char.ToUpperInvariant(x.Name[0])}{x.Name.Substring(1)}", ParameterName: x.Name));

        // Should not need to be looked at when it's a RecordDeclarationSyntax
        return namedTypeSymbol switch
        {
            _ when namedTypeSymbol.InstanceConstructors
                .SelectMany(
                    x => x.GetAttributes().Where(y => y.AttributeClass is
                    {
                        ContainingNamespace.Name: Constants.DynamoDBGenerator.Namespace.Attributes,
                        Name: Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerConstructor,
                        ContainingAssembly.Name: Constants.DynamoDBGenerator.AssemblyName
                    }),
                    (x, _) => x
                )
                .FirstOrDefault() is { } ctor => ctor.DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<ConstructorDeclarationSyntax>()
                .Select(
                    x => x.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Select(y => (MemberName: y.Left.ToString(), ParameterName: y.Right.ToString()))
                )
                .FirstOrDefault() ?? Enumerable.Empty<(string, string)>(),
            {IsRecord: true} when namedTypeSymbol.InstanceConstructors[0]
                .DeclaringSyntaxReferences
                .Select(x => x.GetSyntax())
                .OfType<RecordDeclarationSyntax>()
                .FirstOrDefault() is { } ctor => ctor
                .DescendantNodes()
                .OfType<ParameterSyntax>()
                .Select(x => (MemberName: x.Identifier.Text, ParameterName: x.Identifier.Text)),
            _ => Array.Empty<(string, string)>()
        };

    }
    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }
}