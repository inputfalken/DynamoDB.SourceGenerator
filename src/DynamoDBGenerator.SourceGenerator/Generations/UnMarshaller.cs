using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;

namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class UnMarshaller
{
    private const string DataMember = "dataMember";
    private const string Dict = "dict";
    private static readonly Func<ITypeSymbol, string> GetDeserializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory(null, SymbolEqualityComparer.IncludeNullability);
    private const string UnMarshallerClass = $"_{Marshaller.UnmarshalMethodName}_";
    private const string Value = "attributeValue";
    private static IEnumerable<(bool useParentheses, IEnumerable<string> assignments)> Assignments(TypeIdentifier typeIdentifier, (DynamoDbDataMember DDB, string MethodCall, string Name)[] assignments)
    {
        const string indent = "    ";
        var type = typeIdentifier.TypeSymbol;
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
    internal static IEnumerable<string> CreateClass(DynamoDBMarshallerArguments[] arguments,
        Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
    {
        return $"file static class {UnMarshallerClass}".CreateScope(CreateTypeContents(arguments, getDynamoDbProperties, options));
    }
    private static CodeFactory CreateCode(TypeIdentifier typeIdentifier, Func<ITypeSymbol, DynamoDbDataMember[]> fn, MarshallerOptions options)
    {
        var assignments = fn(typeIdentifier.TypeSymbol)
            .Select(x => (DDB: x, MethodCall: InvokeUnmarshallMethod(x.DataMember.TypeIdentifier, $"{Dict}.GetValueOrDefault(\"{x.AttributeName}\")", $"\"{x.DataMember.Name}\"", options), x.DataMember.Name))
            .ToArray();

        var blockBody =
            $"if ({Dict} is null)"
                .CreateScope(typeIdentifier.IsSupposedToBeNull ? "return null;" : $"throw {ExceptionHelper.NullExceptionMethod}({DataMember});").Concat(
                    Assignments(typeIdentifier, assignments)
                        .AllAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
                        .SelectMany(x => x)
                        .DefaultIfEmpty("();")
                        // Is needed in order to not perform new entity? where '?' is not allowed in the end of the string.
                        .Prepend(typeIdentifier.TypeSymbol.IsTupleType ? "return" : $"return new {typeIdentifier.AnnotatedString.TrimEnd('?')}")
                );

        var method = $"public static {typeIdentifier.AnnotatedString} {GetDeserializationMethodName(typeIdentifier.TypeSymbol)}(Dictionary<string, AttributeValue>? {Dict}, {options.FullName} {MarshallerOptions.ParamReference}, string? {DataMember} = null)".CreateScope(blockBody);

        return new CodeFactory(method, assignments.Select(x => x.DDB.DataMember.TypeIdentifier));

    }
    private static CodeFactory CreateMethod(TypeIdentifier typeIdentifier, Func<ITypeSymbol, DynamoDbDataMember[]> fn,
        MarshallerOptions options)
    {
        if (options.TryReadConversion(typeIdentifier, Value) is {} conversion)
        {
            var signature = CreateSignature(typeIdentifier, options);
            return typeIdentifier.CanBeNull || typeIdentifier.IsSupposedToBeNull is false
                ? signature
                    .CreateScope(
                        $"if ({Value} is null)"
                            .CreateScope($"throw {ExceptionHelper.NullExceptionMethod}({DataMember});")
                            .Append(
                                $"return {conversion} ?? throw {ExceptionHelper.NullExceptionMethod}({DataMember});"
                            )
                    )
                    .ToConversion()
                : signature
                    .CreateScope(
                        $"if ({Value} is null)"
                            .CreateScope("return null;")
                            .Append($"return {conversion};")
                    )
                    .ToConversion();
        }
        
        return typeIdentifier switch
        {
            SingleGeneric singleGeneric when CreateSignature(singleGeneric, options) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.NULL is true)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {InvokeUnmarshallMethod(singleGeneric.T, Value, DataMember, options)};" )
                        )
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.List or SingleGeneric.SupportedType.ICollection => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.L is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.ToList}({Value}.L, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")});")
                        )
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Array or SingleGeneric.SupportedType.IReadOnlyCollection => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.L is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.ToArray}({Value}.L, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")});")
                        )
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IEnumerable => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.L is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.ToEnumerable}({Value}.L, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(singleGeneric.T, "a", "d", options, "o")});")
                        )
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.TypeSymbol.SpecialType is SpecialType.System_String => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.SS is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{(singleGeneric.T.IsSupposedToBeNull ? "string?" : "string")}>" : null)}({(singleGeneric.T.IsSupposedToBeNull ? $"{Value}.SS" : $"{Value}.SS.Select((y,i) => y ?? throw {ExceptionHelper.NullExceptionMethod}($\"{{{DataMember}}}[UNKNOWN]\")")}));")
                        )
                    .ToConversion(),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.NS is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return new {(singleGeneric.TypeSymbol.TypeKind is TypeKind.Interface ? $"HashSet<{singleGeneric.T.UnannotatedString}>" : null)}({Value}.NS.Select(y => {singleGeneric.T.UnannotatedString}.Parse(y)));")
                        )
                    .ToConversion(singleGeneric),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric, options) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.M is null)"
                            .CreateScope(keyValueGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.ToDictionary}({Value}.M, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "a", "d", options, "o")});")
                        )
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateScope(
                        $"if ({Value} is null || {Value}.M is null)"
                            .CreateScope(keyValueGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.ToLookup}({Value}.M, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "a", "d", options, "o")});")
                        )
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))

            },
            UnknownType => CreateCode(typeIdentifier, fn, options),
            _ => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))
        };

    }

    private static string CreateSignature(TypeIdentifier typeIdentifier, MarshallerOptions options)
    {
        return $"public static {typeIdentifier.AnnotatedString} {GetDeserializationMethodName(typeIdentifier.TypeSymbol)}(AttributeValue? {Value}, {options.FullName} {MarshallerOptions.ParamReference}, string? {DataMember} = null)";
    }
    
    private static IEnumerable<string> CreateTypeContents(IEnumerable<DynamoDBMarshallerArguments> arguments,
        Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
    {
        var hashSet = new HashSet<TypeIdentifier>(TypeIdentifier.Nullable);
        return arguments.SelectMany(x =>
            CodeFactory.Create(
                x.EntityTypeSymbol,
                y => CreateMethod(y, getDynamoDbProperties, options),
                hashSet
            )
        );
    }
    
    private static string InvokeUnmarshallMethod(TypeIdentifier typeIdentifier, string paramReference, string dataMember, MarshallerOptions options, string marshallerOptionsReference = MarshallerOptions.ParamReference)
    {
        if (options.IsUnknown(typeIdentifier) is false) 
            return $"{GetDeserializationMethodName(typeIdentifier.TypeSymbol)}({paramReference}, {marshallerOptionsReference}, {dataMember})";

        return
            $"{GetDeserializationMethodName(typeIdentifier.TypeSymbol)}({paramReference}?.M, {marshallerOptionsReference}, {dataMember})";

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


    internal static IEnumerable<string> RootSignature(TypeIdentifier typeIdentifier)
    {
        return $"public {typeIdentifier.AnnotatedString} {Marshaller.UnmarshalMethodName}(Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> entity)".CreateScope(
            "ArgumentNullException.ThrowIfNull(entity);",
            $"return {UnMarshallerClass}.{GetDeserializationMethodName(typeIdentifier.TypeSymbol)}(entity, {MarshallerOptions.FieldReference});");
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