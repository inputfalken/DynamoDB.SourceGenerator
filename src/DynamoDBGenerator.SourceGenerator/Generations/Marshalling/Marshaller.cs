using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
namespace DynamoDBGenerator.SourceGenerator.Generations.Marshalling;

internal static partial class Marshaller
{
    private const string ClassName = $"_{Constants.DynamoDBGenerator.Marshaller.MarshallMethodName}_";
    private const string DataMember = "dataMember";
    private const string DictionaryReference = "attributeValues";
    private static readonly Func<ITypeSymbol, string> GetSerializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory(null, SymbolEqualityComparer.IncludeNullability);
    private const string ParamReference = "entity";

    internal static IEnumerable<string> CreateClass(DynamoDBMarshallerArguments[] arguments, Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
    {
        return $"file static class {ClassName}".CreateScope(TypeContent(arguments, getDynamoDbProperties, options));
    }
    private static CodeFactory CreateDictionaryMethod(ITypeSymbol typeSymbol, Func<ITypeSymbol, DynamoDbDataMember[]> fn, MarshallerOptions options)
    {
        var properties = fn(typeSymbol)
            .Select(x =>
            {
                var accessPattern = $"{ParamReference}.{x.DataMember.Name}";
                var isNullable = x.DataMember.Type.IsNullable();

                var marshallerInvocation = InvokeMarshallerMethod(x.DataMember.Type, accessPattern, $"\"{x.DataMember.Name}\"", options);

                var assignment = isNullable
                    ? $"if ({x.DataMember.NameAsCamelCase} is not null)"
                        .CreateScope($"{DictionaryReference}[\"{x.AttributeName}\"] = {x.DataMember.NameAsCamelCase};")
                        .Prepend($"var {x.DataMember.NameAsCamelCase} = {marshallerInvocation};")
                    : new[] { $"{DictionaryReference}[\"{x.AttributeName}\"] = {marshallerInvocation};" };
                
                return (
                    dictionaryAssignment: assignment,
                    capacityTernary: isNullable ? x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0") : "1",
                    x.DataMember.Type
                );
            })
            .ToArray();

        var isNullable = typeSymbol.IsNullable();
        var enumerable = Enumerable.Empty<string>();
        if (isNullable)
            enumerable = $"if ({ParamReference} is null)".CreateScope("return null;");
        else if (typeSymbol.IsReferenceType)
            enumerable = $"if ({ParamReference} is null)".CreateScope($"throw {ExceptionHelper.NullExceptionMethod}({DataMember});");

        var body =
            enumerable.Concat(InitializeDictionary(properties.Select(x => x.capacityTernary))
                .Concat(properties.SelectMany(x => x.dictionaryAssignment))
                .Append($"return {DictionaryReference};"));

        var code =
            $"public static Dictionary<string, AttributeValue>{(isNullable ? '?' : null)} {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {options.FullName} {MarshallerOptions.ParamReference}, string? {DataMember} = null)"
                .CreateScope(body);

        return new CodeFactory(code, properties.Select(y => y.Type));

    }

    private static IEnumerable<string> TypeContent(DynamoDBMarshallerArguments[] arguments, Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
    {
        var hashset = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments.SelectMany(x => CodeFactory
                .Create(
                    x.EntityTypeSymbol,
                    y => CreateMethod(y, getDynamoDbProperties, options),
                    hashset
                )
                .Concat(CodeFactory.Create(x.ArgumentType, y => CreateMethod(y, getDynamoDbProperties, options),
                    hashset))
            )
            .Concat(KeyMarshaller.CreateKeys(arguments, getDynamoDbProperties, options));
    }
    private static CodeFactory CreateMethod(ITypeSymbol type, Func<ITypeSymbol, DynamoDbDataMember[]> fn, MarshallerOptions options)
    {
        if (options.TryWriteConversion(type, ParamReference) is {} conversion)
        {
            return type switch
            {
                { IsValueType: true } => type switch
                {
                    { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } => CreateSignature(type, options)
                        .CreateScope($"if ({ParamReference} is null)"
                            .CreateScope("return null;")
                            .Append($"return {conversion};")
                        )
                        .ToConversion(),
                    _ => CreateSignature(type, options)
                        .CreateScope($"return {conversion};")
                        .ToConversion()
                },
                { IsReferenceType: true } => type switch
                {
                    { NullableAnnotation: NullableAnnotation.None or NullableAnnotation.Annotated } => CreateSignature(
                            type, options)
                        .CreateScope($"if ({ParamReference} is null)".CreateScope("return null;")
                            .Append($"return {conversion};"))
                        .ToConversion(),
                    _ => CreateSignature(type, options)
                        .CreateScope(
                            $"if ({ParamReference} is null)".CreateScope($"throw {ExceptionHelper.NullExceptionMethod}({DataMember});").Append($"return {conversion};")
                            )
                        .ToConversion()
                },
                _ => throw new ArgumentException(
                    $"Neither ValueType or ReferenceType could be resolved for conversion. type '{type.ToDisplayString()}'.")
            };
        }

        return type.TypeIdentifier() switch
        {
            SingleGeneric singleGeneric when CreateSignature(singleGeneric.TypeSymbol, options) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateScope(
                        $"if ({ParamReference} is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {InvokeMarshallerMethod(singleGeneric.T, $"{ParamReference}.Value", DataMember, options)};")
                    ).ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Array => signature
                    .CreateScope(
                        $"if ({ParamReference} is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.FromArray}({ParamReference}, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeMarshallerMethod(singleGeneric.T, "a", "d", options, "o")}{(singleGeneric.T.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)});")
                    ).ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.List => signature
                    .CreateScope(
                        $"if ({ParamReference} is null)"
                            .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.FromList}({ParamReference}, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeMarshallerMethod(singleGeneric.T, "a", "d", options, "o")}{(singleGeneric.T.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)});")
                    ).ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => signature
                        .CreateScope(
                            $"if ({ParamReference} is null)"
                                .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                                .Append($"return {AttributeValueUtilityFactory.FromEnumerable}({ParamReference}, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeMarshallerMethod(singleGeneric.T, "a", "d", options, "o")}{(singleGeneric.T.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)});")
                        ).ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String
                    => signature
                        .CreateScope(
                            $"if ({ParamReference} is null)"
                                .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                                .Append($"return new {Constants.AWSSDK_DynamoDBv2.AttributeValue} {{ SS = new List<{(singleGeneric.T.IsNullable() ? "string?" : "string")}>({(singleGeneric.T.IsNullable() ? ParamReference : $"{ParamReference}.Select((y,i) => y ?? throw {ExceptionHelper.NullExceptionMethod}($\"{{{DataMember}}}[UNKNOWN]\"))")})}};")
                        )
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric()
                    => signature
                        .CreateScope(
                            $"if ({ParamReference} is null)"
                                .CreateScope(singleGeneric.ReturnNullOrThrow(DataMember))
                                .Append($"return new {Constants.AWSSDK_DynamoDBv2.AttributeValue} {{ NS = new List<string>({ParamReference}.Select(y => y.ToString())) }};")
                        )
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric.TypeSymbol, options) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateScope(
                        $"if ({ParamReference} is null)"
                            .CreateScope(keyValueGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.FromDictionary}({ParamReference}, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeMarshallerMethod(keyValueGeneric.TValue, "a", "d", options, "o")}{(keyValueGeneric.TValue.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)});")
                    )
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateScope(
                        $"if ({ParamReference} is null)"
                            .CreateScope(keyValueGeneric.ReturnNullOrThrow(DataMember))
                            .Append($"return {AttributeValueUtilityFactory.FromLookup}({ParamReference}, {MarshallerOptions.ParamReference}, {DataMember}, static (a, o, d) => {InvokeMarshallerMethod(keyValueGeneric.TValue, "a", "d", options, "o")}{(keyValueGeneric.TValue.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)});")
                    )
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))
            },
            UnknownType unknownType => CreateDictionaryMethod(unknownType.TypeSymbol, fn, options),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))

        };

    }
    private static string CreateSignature(ITypeSymbol typeSymbol, MarshallerOptions options)
    {
        return typeSymbol.IsNullable()
            ? $"public static {Constants.AWSSDK_DynamoDBv2.AttributeValue}? {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {options.FullName} {MarshallerOptions.ParamReference}, string? {DataMember} = null)"
            : $"public static {Constants.AWSSDK_DynamoDBv2.AttributeValue} {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {options.FullName} {MarshallerOptions.ParamReference}, string? {DataMember} = null)";
    }

    private static IEnumerable<string> InitializeDictionary(IEnumerable<string> capacityCalculations)
    {
        var capacityCalculation = string.Join(" + ", capacityCalculations);
        if (capacityCalculation is "")
        {
            yield return $"var {DictionaryReference} = new Dictionary<string, {Constants.AWSSDK_DynamoDBv2.AttributeValue}>(0);";
        }
        else
        {
            yield return $"var capacity = {capacityCalculation};";
            yield return $"var {DictionaryReference} = new Dictionary<string, {Constants.AWSSDK_DynamoDBv2.AttributeValue}>(capacity);";
        }
    }

    internal static string InvokeMarshallerMethod(ITypeSymbol typeSymbol, string parameterReference, string dataMember, MarshallerOptions options, string optionParam = MarshallerOptions.ParamReference)
    {
        var invocation = $"{ClassName}.{GetSerializationMethodName(typeSymbol)}({parameterReference}, {optionParam}, {dataMember})";

        if (options.IsConvertable(typeSymbol))
            return invocation;

        if (typeSymbol.TypeIdentifier() is UnknownType)
            return typeSymbol.IsNullable()
                ? $"{AttributeValueUtilityFactory.ToAttributeValue}({invocation})"
                : $"new {Constants.AWSSDK_DynamoDBv2.AttributeValue} {{ M = {invocation} }}";

        return invocation;
    }

    internal static IEnumerable<string> RootSignature(ITypeSymbol typeSymbol, string rootTypeName)
    {

        return $"public Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> {Constants.DynamoDBGenerator.Marshaller.MarshallMethodName}({rootTypeName} {ParamReference})"
            .CreateScope(
                $"ArgumentNullException.ThrowIfNull({ParamReference});",
                $"return {ClassName}.{GetSerializationMethodName(typeSymbol)}({ParamReference}, {MarshallerOptions.FieldReference});"
            );
    }

    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }
}