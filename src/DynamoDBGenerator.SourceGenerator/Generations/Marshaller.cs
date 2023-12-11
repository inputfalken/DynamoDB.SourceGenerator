using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.ExceptionHelper;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class Marshaller
{
    private const string ClassName = $"_{nameof(Marshaller)}_";
    private static readonly Func<ITypeSymbol, string> GetSerializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("_M", SymbolEqualityComparer.IncludeNullability);

    internal static IEnumerable<string> CreateClass(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        return $"private static class {ClassName}".CreateBlock(CreateMarshaller(arguments, getDynamoDbProperties));
    }

    internal static IEnumerable<string> RootSignature(ITypeSymbol typeSymbol, string rootTypeName)
    {
        return $"public Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> {Constants.DynamoDBGenerator.Marshaller.MarshallMethodName}({rootTypeName} entity)"
            .CreateBlock(
                "ArgumentNullException.ThrowIfNull(entity);",
                $"return {ClassName}.{GetSerializationMethodName(typeSymbol)}(entity);"
            );
    }
    
    internal static string InvokeMarshallerMethod(ITypeSymbol typeSymbol, string parameterReference, string dataMember)
    {
        var invocation = $"{ClassName}.{GetSerializationMethodName(typeSymbol)}({parameterReference}, {dataMember})";

        if (DynamoDbMarshaller.TypeIdentifier(typeSymbol) is UnknownType)
            return typeSymbol.IsNullable() is false // Can get rid of this if the signature accepts nullable
                ? $"new AttributeValue {{ M = {invocation} ?? throw {NullExceptionMethod}({dataMember}) }}"
                : $"{Constants.DynamoDBGenerator.AttributeValueUtilityFactory.ToAttributeValue}({invocation})";

        return invocation;
    }

    private static IEnumerable<string> CreateMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashset = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments.SelectMany(x => Conversion
                .ConversionMethods(
                    x.EntityTypeSymbol,
                    y => CreateMethod(y, getDynamoDbProperties),
                    hashset
                )
                .Concat(Conversion.ConversionMethods(x.ArgumentType, y => CreateMethod(y, getDynamoDbProperties), hashset))
            )
            .SelectMany(x => x.Code);
    }

    private static Conversion CreateMethod(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        const string param = "entity";
        const string dataMember = "dataMember";

        static string CreateSignature(TypeIdentifier typeIdentifier) => typeIdentifier.TypeSymbol.IsNullable()
            ? $"public static AttributeValue? {GetSerializationMethodName(typeIdentifier.TypeSymbol)}({DynamoDbMarshaller.TypeName(typeIdentifier.TypeSymbol).annotated} {param}, string? {dataMember} = null)"
            : $"public static AttributeValue {GetSerializationMethodName(typeIdentifier.TypeSymbol)}({DynamoDbMarshaller.TypeName(typeIdentifier.TypeSymbol).annotated} {param}, string? {dataMember} = null)";

        static string Else(TypeIdentifier typeIdentifier) => typeIdentifier.TypeSymbol.IsNullable() ? "null" : $"throw {NullExceptionMethod}({dataMember})";

        return DynamoDbMarshaller.TypeIdentifier(type) switch
        {
            BaseType baseType when CreateSignature(baseType) is var signature => baseType.Type switch
            {
                BaseType.SupportedType.String => signature.CreateBlock($"return {param} is not null ? new AttributeValue {{ S = {param} }} : {Else(baseType)};").ToConversion(),
                BaseType.SupportedType.Bool => signature.CreateBlock($"return new AttributeValue {{ BOOL = {param} }};").ToConversion(),
                BaseType.SupportedType.Int16
                    or BaseType.SupportedType.Int32
                    or BaseType.SupportedType.Int64
                    or BaseType.SupportedType.UInt16
                    or BaseType.SupportedType.UInt32
                    or BaseType.SupportedType.UInt64
                    or BaseType.SupportedType.Double
                    or BaseType.SupportedType.Decimal
                    or BaseType.SupportedType.Single
                    or BaseType.SupportedType.SByte
                    or BaseType.SupportedType.Byte
                    => signature.CreateBlock($"return new AttributeValue {{ N = {param}.ToString() }};").ToConversion(),
                BaseType.SupportedType.Char => signature.CreateBlock($"return new AttributeValue {{ S = {param}.ToString() }};").ToConversion(),
                BaseType.SupportedType.DateOnly or BaseType.SupportedType.DateTimeOffset or BaseType.SupportedType.DateTime
                    => signature.CreateBlock($"return new AttributeValue {{ S = {param}.ToString(\"O\") }};").ToConversion(),
                BaseType.SupportedType.Enum => signature.CreateBlock($"return new AttributeValue {{ N = ((int){param}).ToString() }};").ToConversion(),
                BaseType.SupportedType.MemoryStream => signature.CreateBlock($"return {param} is not null ? new AttributeValue {{ B = {param} }} : {Else(baseType)};").ToConversion(),
                _ => throw UncoveredConversionException(baseType, nameof(CreateMethod))
            },
            SingleGeneric singleGeneric when CreateSignature(singleGeneric) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateBlock($"return {param} is not null ? {InvokeMarshallerMethod(singleGeneric.T, $"{param}.Value", dataMember)} : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => signature
                        .CreateBlock(
                            $"return {param} is not null ? new AttributeValue {{ L = new List<AttributeValue>({param}.Select((y, i) => {InvokeMarshallerMethod(singleGeneric.T, "y", $"$\"{{{dataMember}}}[{{i.ToString()}}]\"")} {(singleGeneric.T.IsNullable() ? " ?? new AttributeValue { NULL = true }" : null)})) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String
                    => signature
                        .CreateBlock(
                            $"return {param} is not null ? new AttributeValue {{ SS = new List<{(singleGeneric.T.IsNullable() ? "string?" : "string")}>({(singleGeneric.T.IsNullable() ? param : $"{param}.Select((y,i) => y ?? throw {NullExceptionMethod}($\"{{{dataMember}}}[UNKNOWN]\"))")})}} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric()
                    => signature
                        .CreateBlock($"return {param} is not null ? new AttributeValue {{ NS = new List<string>({param}.Select(y => y.ToString())) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(CreateMethod))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(CreateMethod))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateBlock(
                        $"return {param} is not null ? new AttributeValue {{ M = {param}.ToDictionary(y => y.Key, y => {InvokeMarshallerMethod(keyValueGeneric.TValue, "y.Value", dataMember)}) }} : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateBlock(
                        $"return {param} is not null ? new AttributeValue {{ M = {param}.ToDictionary(y => y.Key, y => new AttributeValue {{ L = new List<AttributeValue>(y.Select(z => {InvokeMarshallerMethod(keyValueGeneric.TValue, "z", dataMember)})) }}) }} : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(CreateMethod))
            },
            UnknownType unknownType => CreateDictionaryMethod(unknownType.TypeSymbol),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(CreateMethod))

        };

        Conversion CreateDictionaryMethod(ITypeSymbol typeSymbol)
        {
            const string paramReference = "entity";
            const string dictionaryReference = "attributeValues";

            var properties = fn(typeSymbol)
                .Select(x =>
                {
                    var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                    return (
                        dictionaryAssignment: $"if ({InvokeMarshallerMethod(x.DataMember.Type, accessPattern, $"\"{x.DataMember.Name}\"")} is {{ }} {x.DataMember.Name})".CreateBlock(
                            $"{dictionaryReference}.Add(\"{x.AttributeName}\", {x.DataMember.Name});"),
                        capacityTernary: x.DataMember.Type.IsNullable() ? x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0") : "1",
                        x.DataMember.Type
                    );
                })
                .ToArray();

            var isNullable = typeSymbol.IsNullable();
            var enumerable = Enumerable.Empty<string>();
            if (isNullable)
                enumerable = $"if ({paramReference} is null)".CreateBlock("return null;");
            else if (typeSymbol.IsReferenceType)
                enumerable = $"if ({paramReference} is null)".CreateBlock($"throw {NullExceptionMethod}({dataMember});");

            var body =
                enumerable.Concat(InitializeDictionary(properties.Select(x => x.capacityTernary))
                    .Concat(properties.SelectMany(x => x.dictionaryAssignment))
                    .Append($"return {dictionaryReference};"));

            var code =
                $"public static Dictionary<string, AttributeValue>{(isNullable ? '?' : null)} {GetSerializationMethodName(type)}({DynamoDbMarshaller.TypeName(type).annotated} {paramReference}, string? {dataMember} = null)"
                    .CreateBlock(body);

            return new Conversion(code, properties.Select(y => y.Type));

            static IEnumerable<string> InitializeDictionary(IEnumerable<string> capacityCalculations)
            {
                var capacityCalculation = string.Join(" + ", capacityCalculations);
                if (capacityCalculation is "")
                {
                    yield return $"var {dictionaryReference} = new Dictionary<string, AttributeValue>(0);";
                }
                else
                {
                    yield return $"var capacity = {capacityCalculation};";
                    yield return $"var {dictionaryReference} = new Dictionary<string, AttributeValue>(capacity);";
                }
            }
        }
    }

    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }
}