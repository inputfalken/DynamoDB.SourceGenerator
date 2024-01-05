using System.Collections;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;

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
    internal static IEnumerable<string> CreateClass(IEnumerable<DynamoDBMarshallerArguments> arguments,
        Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        return $"private static class {UnMarshallerClass}".CreateBlock(CreateUnMarshaller(arguments, getDynamoDbProperties, options));
    }
    private static Conversion CreateCode(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn, MarshallerOptions options)
    {
        var assignments = fn(type)
            .Select(x => (DDB: x, MethodCall: InvokeUnmarshallMethod(x.DataMember.Type, $"{Dict}.GetValueOrDefault(\"{x.AttributeName}\")", $"\"{x.DataMember.Name}\"", options), x.DataMember.Name))
            .ToArray();

        var typeName = type.Representation();
        var blockBody =
            $"if ({Dict} is null)"
                .CreateBlock(type.IsNullable() ? "return null;" : $"throw {ExceptionHelper.NullExceptionMethod}({DataMember});").Concat(
                    Assignments(type, assignments)
                        .AllAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
                        .SelectMany(x => x)
                        .DefaultIfEmpty("();")
                        // Is needed in order to not perform new entity? where '?' is not allowed in the end of the string.
                        .Prepend(type.IsTupleType ? "return" : $"return new {typeName.annotated.TrimEnd('?')}")
                );

        var method = $"public static {typeName.annotated} {GetDeserializationMethodName(type)}(Dictionary<string, AttributeValue>? {Dict}, {MarshallerOptions.Name} {MarshallerOptions.ParamReference}, string? {DataMember} = null)".CreateBlock(blockBody);

        return new Conversion(method, assignments.Select(x => x.DDB.DataMember.Type));

    }
    private static Conversion CreateMethod(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn,
        MarshallerOptions options)
    {

        if (options.TryReadConversion(type, Value) is {} conversion)
        {
            if (type.IsNullable())
                return CreateSignature(type)
                    .CreateBlock($"return {Value} is not null ? ({conversion}) : null;")
                    .ToConversion();

            return CreateSignature(type)
                .CreateBlock($"return {Value} is not null && ({conversion}) is {{ }} x ? x : throw {ExceptionHelper.NullExceptionMethod}({DataMember});")
                .ToConversion();
        }
        
        return type.TypeIdentifier() switch
        {
            SingleGeneric singleGeneric when CreateSignature(singleGeneric.TypeSymbol) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateBlock($"return {Value} is not null and {{ NULL: false }} ? {InvokeUnmarshallMethod(singleGeneric.T, Value, DataMember, options)} : null;")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.List or SingleGeneric.SupportedType.ICollection => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? {AttributeValueUtilityFactory.ToList}(x, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")}) : {Else(singleGeneric.TypeSymbol)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Array or SingleGeneric.SupportedType.IReadOnlyCollection => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? {AttributeValueUtilityFactory.ToArray}(x, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")}) : {Else(singleGeneric.TypeSymbol)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IEnumerable => signature
                    .CreateBlock($"return {Value} is {{ L: {{ }} x }} ? {AttributeValueUtilityFactory.ToEnumerable}(x, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")}) : {Else(singleGeneric.TypeSymbol)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String => signature
                    .CreateBlock(
                        $"return {Value} is {{ SS : {{ }} x }} ? new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{(singleGeneric.T.IsNullable() ? "string?" : "string")}>" : null)}({(singleGeneric.T.IsNullable() ? "x" : $"x.Select((y,i) => y ?? throw {ExceptionHelper.NullExceptionMethod}($\"{{{DataMember}}}[UNKNOWN]\")")})) : {Else(singleGeneric.TypeSymbol)};")
                    .ToConversion(),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric() => signature
                    .CreateBlock(
                        $"return {Value} is {{ NS : {{ }} x }} ? new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{singleGeneric.T.Representation().original}>" : null)}(x.Select(y => {singleGeneric.T.Representation().original}.Parse(y))) : {Else(singleGeneric.TypeSymbol)};")
                    .ToConversion(singleGeneric.TypeSymbol),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric.TypeSymbol) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateBlock($"return {Value} is {{ M: {{ }} x }} ? {AttributeValueUtilityFactory.ToDictionary}(x, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "a", "d", options, "o")}) : {Else(keyValueGeneric.TypeSymbol)};")
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateBlock($"return {Value} is {{ M: {{ }} x }} ? {AttributeValueUtilityFactory.ToLookup}(x, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "a", "d", options, "o")}) : {Else(keyValueGeneric.TypeSymbol)};")
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))

            },
            UnknownType => CreateCode(type, fn, options),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))
        };

    }

    private static IEnumerable<string> CreateCollection(SingleGeneric type, MarshallerOptions options, bool isReadOnly)
    {
        var ifCase = $"if ({Value}?.L is null)".CreateBlock(type.TypeSymbol.IsNullable()
            ? "return null;"
            : $"throw {ExceptionHelper.NullExceptionMethod}({DataMember});");
        
        foreach (var s in ifCase)
            yield return s;

        yield return $"var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan({Value}.L);";
        IEnumerable<string> forLoop;
        var typeRepresentation = type.T.Representation().annotated;
        if (isReadOnly)
        {
            yield return $"var elements = new {typeRepresentation}[span.Length];";
            forLoop = "for (var i  = 0; i < span.Length; i++)"
                .CreateBlock($"elements[i] = {InvokeUnmarshallMethod(type.T, "span[i]", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"", options)};");
        }
        else
        {
            yield return $"var elements = new List<{typeRepresentation}>(span.Length);";
            forLoop = "for (var i  = 0; i < span.Length; i++)".CreateBlock($"elements.Add({InvokeUnmarshallMethod(type.T, "span[i]", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"", options)});");
        }
        
        foreach (var s in forLoop)
            yield return s;

        yield return "return elements;";
    }

    private static string CreateSignature(ITypeSymbol typeSymbol)
    {
        return $"public static {typeSymbol.Representation().annotated} {GetDeserializationMethodName(typeSymbol)}(AttributeValue? {Value}, {MarshallerOptions.Name} {MarshallerOptions.ParamReference}, string? {DataMember} = null)";
    }
    private static IEnumerable<string> CreateUnMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments,
        Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);
        return arguments.SelectMany(x =>
                Conversion.ConversionMethods(
                    x.EntityTypeSymbol,
                    y => CreateMethod(y, getDynamoDbProperties, options),
                    hashSet
                )
            )
            .SelectMany(x => x.Code);
    }
    private static string Else(ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsNullable() ? "null" : $"throw {ExceptionHelper.NullExceptionMethod}({DataMember})";
    }

    private static string InvokeUnmarshallMethod(ITypeSymbol typeSymbol, string paramReference, string dataMember, MarshallerOptions options, string marshallerOptionsReference = MarshallerOptions.ParamReference)
    {
        if (options.IsConvertable(typeSymbol))
            return $"{GetDeserializationMethodName(typeSymbol)}({paramReference}, {marshallerOptionsReference}, {dataMember})";
        
        return typeSymbol.TypeIdentifier() is UnknownType
            ? $"{GetDeserializationMethodName(typeSymbol)}({paramReference}?.M, {marshallerOptionsReference}, {dataMember})"
            : $"{GetDeserializationMethodName(typeSymbol)}({paramReference}, {marshallerOptionsReference}, {dataMember})";

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
            $"return {UnMarshallerClass}.{GetDeserializationMethodName(typeSymbol)}(entity, {MarshallerOptions.FieldReference});");
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
                        ContainingNamespace.Name: Namespace.Attributes,
                        Name: Constants.DynamoDBGenerator.Attribute.DynamoDBMarshallerConstructor,
                        ContainingAssembly.Name: AssemblyName
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