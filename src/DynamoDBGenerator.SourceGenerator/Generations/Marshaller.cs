using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class Marshaller
{
    private const string ClassName = $"_{Constants.DynamoDBGenerator.Marshaller.MarshallMethodName}_";
    private const string DataMember = "dataMember";
    private const string DictionaryReference = "attributeValues";
    private static readonly Func<ITypeSymbol, string> GetSerializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("_M", SymbolEqualityComparer.IncludeNullability);
    private const string ParamReference = "entity";

    internal static IEnumerable<string> CreateClass(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        return $"private static class {ClassName}".CreateBlock(CreateMarshaller(arguments, getDynamoDbProperties, options));
    }
    private static Conversion CreateDictionaryMethod(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn, MarshallerOptions options)
    {

        var properties = fn(typeSymbol)
            .Select(x =>
            {
                var accessPattern = $"{ParamReference}.{x.DataMember.Name}";
                return (
                    dictionaryAssignment: $"if ({InvokeMarshallerMethod(x.DataMember.Type, accessPattern, $"\"{x.DataMember.Name}\"", options)} is {{ }} {x.DataMember.Name})".CreateBlock(
                        $"{DictionaryReference}.Add(\"{x.AttributeName}\", {x.DataMember.Name});"),
                    capacityTernary: x.DataMember.Type.IsNullable() ? x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0") : "1",
                    x.DataMember.Type
                );
            })
            .ToArray();

        var isNullable = typeSymbol.IsNullable();
        var enumerable = Enumerable.Empty<string>();
        if (isNullable)
            enumerable = $"if ({ParamReference} is null)".CreateBlock("return null;");
        else if (typeSymbol.IsReferenceType)
            enumerable = $"if ({ParamReference} is null)".CreateBlock($"throw {ExceptionHelper.NullExceptionMethod}({DataMember});");

        var body =
            enumerable.Concat(InitializeDictionary(properties.Select(x => x.capacityTernary))
                .Concat(properties.SelectMany(x => x.dictionaryAssignment))
                .Append($"return {DictionaryReference};"));

        var code =
            $"public static Dictionary<string, AttributeValue>{(isNullable ? '?' : null)} {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {MarshallerOptions.Name} {MarshallerOptions.PropertyName}, string? {DataMember} = null)"
                .CreateBlock(body);

        return new Conversion(code, properties.Select(y => y.Type));

    }

    private static IEnumerable<string> CreateMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties, MarshallerOptions options)
    {
        var hashset = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments.SelectMany(x => Conversion
                .ConversionMethods(
                    x.EntityTypeSymbol,
                    y => CreateMethod(y, getDynamoDbProperties, options),
                    hashset
                )
                .Concat(Conversion.ConversionMethods(x.ArgumentType, y => CreateMethod(y, getDynamoDbProperties, options), hashset))
            )
            .SelectMany(x => x.Code);
    }
    private static Conversion CreateMethod(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn, MarshallerOptions options)
    {
        if (options.AccessConverterWrite(type, ParamReference) is {} a)
        {
            if (type is {IsReferenceType:true} or {OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
            {
                
                if (type.IsNullable())
                    return CreateSignature(type)
                        .CreateBlock($"return {ParamReference} is not null ? {a} : null;")
                        .ToConversion();
                
                return CreateSignature(type)
                    .CreateBlock($"return {ParamReference} is not null ? {a} : throw {ExceptionHelper.NullExceptionMethod}({DataMember});")
                    .ToConversion();
            }

            return CreateSignature(type)
                .CreateBlock($"return {a};")
                .ToConversion();
        }

        return type.TypeIdentifier() switch
        {
            BaseType baseType when CreateSignature(baseType.TypeSymbol) is var signature => baseType.Type switch
            {
                BaseType.SupportedType.DateOnly => signature.CreateBlock($"return new AttributeValue {{ S = {ParamReference}.ToString(\"O\") }};").ToConversion(),
                BaseType.SupportedType.Enum => signature.CreateBlock($"return new AttributeValue {{ N = ((int){ParamReference}).ToString() }};").ToConversion(),
                _ => throw UncoveredConversionException(baseType, nameof(CreateMethod))
            },
            SingleGeneric singleGeneric when CreateSignature(singleGeneric.TypeSymbol) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateBlock($"return {ParamReference} is not null ? {InvokeMarshallerMethod(singleGeneric.T, $"{ParamReference}.Value", DataMember, options)} : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => signature
                        .CreateBlock(
                            $"return {ParamReference} is not null ? new AttributeValue {{ L = new List<AttributeValue>({ParamReference}.Select((y, i) => {InvokeMarshallerMethod(singleGeneric.T, "y", $"$\"{{{DataMember}}}[{{i.ToString()}}]\"", options)} {(singleGeneric.T.IsNullable() ? $" ?? {AttributeValueUtilityFactory.Null}" : null)})) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String
                    => signature
                        .CreateBlock(
                            $"return {ParamReference} is not null ? new AttributeValue {{ SS = new List<{(singleGeneric.T.IsNullable() ? "string?" : "string")}>({(singleGeneric.T.IsNullable() ? ParamReference : $"{ParamReference}.Select((y,i) => y ?? throw {ExceptionHelper.NullExceptionMethod}($\"{{{DataMember}}}[UNKNOWN]\"))")})}} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric()
                    => signature
                        .CreateBlock($"return {ParamReference} is not null ? new AttributeValue {{ NS = new List<string>({ParamReference}.Select(y => y.ToString())) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric.TypeSymbol) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateBlock(
                        $"return {ParamReference} is not null ? new AttributeValue {{ M = {ParamReference}.ToDictionary(y => y.Key, y => {InvokeMarshallerMethod(keyValueGeneric.TValue, "y.Value", DataMember, options)}) }} : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateBlock(
                        $"return {ParamReference} is not null ? new AttributeValue {{ M = {ParamReference}.ToDictionary(y => y.Key, y => new AttributeValue {{ L = new List<AttributeValue>(y.Select(z => {InvokeMarshallerMethod(keyValueGeneric.TValue, "z", DataMember, options)})) }}) }} : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))
            },
            UnknownType unknownType => CreateDictionaryMethod(unknownType.TypeSymbol, fn, options),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))

        };

    }
    private static string CreateSignature(ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsNullable()
            ? $"public static AttributeValue? {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {MarshallerOptions.Name} {MarshallerOptions.PropertyName}, string? {DataMember} = null)"
            : $"public static AttributeValue {GetSerializationMethodName(typeSymbol)}({typeSymbol.Representation().annotated} {ParamReference}, {MarshallerOptions.Name} {MarshallerOptions.PropertyName}, string? {DataMember} = null)";
    }

    private static string Else(TypeIdentifier typeIdentifier)
    {
        return typeIdentifier.TypeSymbol.IsNullable() ? "null" : $"throw {ExceptionHelper.NullExceptionMethod}({DataMember})";
    }
    private static IEnumerable<string> InitializeDictionary(IEnumerable<string> capacityCalculations)
    {
        var capacityCalculation = string.Join(" + ", capacityCalculations);
        if (capacityCalculation is "")
        {
            yield return $"var {DictionaryReference} = new Dictionary<string, AttributeValue>(0);";
        }
        else
        {
            yield return $"var capacity = {capacityCalculation};";
            yield return $"var {DictionaryReference} = new Dictionary<string, AttributeValue>(capacity);";
        }
    }

    internal static string InvokeMarshallerMethod(ITypeSymbol typeSymbol, string parameterReference, string dataMember, MarshallerOptions options)
    {
        var invocation = $"{ClassName}.{GetSerializationMethodName(typeSymbol)}({parameterReference}, {MarshallerOptions.PropertyName}, {dataMember})";

        if (options.IsConvertable(typeSymbol))
            return invocation;

        if (typeSymbol.TypeIdentifier() is UnknownType)
            return typeSymbol.IsNullable() is false // Can get rid of this if the signature accepts nullable
                ? $"new AttributeValue {{ M = {invocation} ?? throw {ExceptionHelper.NullExceptionMethod}({dataMember}) }}"
                : $"{AttributeValueUtilityFactory.ToAttributeMap}({invocation})";

        return invocation;
    }

    internal static IEnumerable<string> RootSignature(ITypeSymbol typeSymbol, string rootTypeName)
    {

        return $"public Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> {Constants.DynamoDBGenerator.Marshaller.MarshallMethodName}({rootTypeName} {ParamReference})"
            .CreateBlock(
                $"ArgumentNullException.ThrowIfNull({ParamReference});",
                $"return {ClassName}.{GetSerializationMethodName(typeSymbol)}({ParamReference}, {MarshallerOptions.PropertyName});"
            );
    }

    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }
}