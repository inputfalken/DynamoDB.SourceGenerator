using System.Runtime.CompilerServices;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.ExceptionHelper;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
using KeyValueGeneric = DynamoDBGenerator.SourceGenerator.Types.KeyValueGeneric;
namespace DynamoDBGenerator.SourceGenerator;

public static class DynamoDbMarshaller
{
    private static readonly Func<ITypeSymbol, string> GetAttributeExpressionNameTypeName;
    private static readonly Func<ITypeSymbol, string> GetAttributeExpressionValueTypeName;
    private static readonly Func<ITypeSymbol, string> GetAttributeValueInterfaceName;
    private static readonly Func<ITypeSymbol, string> GetDeserializationMethodName;
    private static readonly Func<ITypeSymbol, (string annotated, string original)> GetTypeName;
    private static readonly Func<ITypeSymbol, string> GetKeysMethodName;
    private static readonly Func<ITypeSymbol, string> GetSerializationMethodName;
    private static readonly Func<ITypeSymbol, TypeIdentifier> GetTypeIdentifier;
    private const string MarshallerClass = "_Marshaller_";
    private const string UnMarshallerClass = "_Unmarshaller_";

    static DynamoDbMarshaller()
    {
        GetTypeName = TypeExtensions.GetTypeIdentifier(SymbolEqualityComparer.IncludeNullability);
        GetTypeIdentifier = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, x => x.GetKnownType());
        GetDeserializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("_U", SymbolEqualityComparer.IncludeNullability);
        GetKeysMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("Keys", SymbolEqualityComparer.IncludeNullability);
        GetSerializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("_M", SymbolEqualityComparer.IncludeNullability);
        GetAttributeExpressionNameTypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Names", SymbolEqualityComparer.Default);
        GetAttributeExpressionValueTypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", SymbolEqualityComparer.Default);
        GetAttributeValueInterfaceName = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, x => $"{AttributeExpressionValueTrackerInterface}<{GetTypeName(x).annotated}>");
    }

    private static IEnumerable<string> CreateExpressionAttributeName(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return arguments
            .SelectMany(x => Conversion.ConversionMethods(x.EntityTypeSymbol, y => ExpressionAttributeName(y, getDynamoDbProperties), hashSet)).SelectMany(x => x.Code);

    }
    private static IEnumerable<string> CreateExpressionAttributeValue(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return arguments
            .SelectMany(x => Conversion.ConversionMethods(x.ArgumentType, y => ExpressionAttributeValue(y, getDynamoDbProperties), hashSet)).SelectMany(x => x.Code);
    }
    private static IEnumerable<string> CreateImplementations(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        foreach (var argument in arguments)
        {
            var rootTypeName = GetTypeName(argument.EntityTypeSymbol).annotated;
            var valueTrackerTypeName = GetAttributeExpressionValueTypeName(argument.ArgumentType);
            var nameTrackerTypeName = GetAttributeExpressionNameTypeName(argument.EntityTypeSymbol);

            var interfaceImplementation = $"public Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> {MarshallMethodName}({rootTypeName} entity)"
                .CreateBlock(
                    "ArgumentNullException.ThrowIfNull(entity);",
                    $"return {MarshallerClass}.{GetSerializationMethodName(argument.EntityTypeSymbol)}(entity);"
                )
                .Concat($"public {rootTypeName} {UnmarshalMethodName}(Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> entity)".CreateBlock(
                    "ArgumentNullException.ThrowIfNull(entity);",
                    $"return {UnMarshallerClass}.{GetDeserializationMethodName(argument.EntityTypeSymbol)}(entity);")
                )
                .Concat($"public {IndexKeyMarshallerInterface} IndexKeyMarshaller(string index)".CreateBlock(
                        "ArgumentNullException.ThrowIfNull(index);",
                        $"return new {Constants.DynamoDBGenerator.IndexKeyMarshallerImplementationTypeName}({GetKeysMethodName(argument.EntityTypeSymbol)}, index);"
                    )
                )
                .Concat($"public {valueTrackerTypeName} {AttributeExpressionValueTrackerMethodName}()".CreateBlock(
                        "var incrementer = new DynamoExpressionValueIncrementer();",
                        $"return new {valueTrackerTypeName}(incrementer.GetNext);"
                    )
                )
                .Append($"public {nameTrackerTypeName} {AttributeExpressionNameTrackerMethodName}() => new {nameTrackerTypeName}(null);")
                .Append($"public {KeyMarshallerInterface} PrimaryKeyMarshaller {{ get; }} = new {Constants.DynamoDBGenerator.KeyMarshallerImplementationTypeName}({GetKeysMethodName(argument.EntityTypeSymbol)});");

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{rootTypeName}, {GetTypeName(argument.ArgumentType).annotated}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return
                $"public {Interface}<{rootTypeName}, {GetTypeName(argument.ArgumentType).annotated}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}();";

            foreach (var s in classImplementation)
                yield return s;

        }
    }
    private static IEnumerable<string> CreateKeys(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments
            .SelectMany(x => Conversion.ConversionMethods(x.EntityTypeSymbol, y => StaticAttributeValueDictionaryKeys(y, getDynamoDbProperties), hashSet)).SelectMany(x => x.Code);
    }

    private static IEnumerable<string> CreateMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashset = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments.SelectMany(x => Conversion
                .ConversionMethods(
                    x.EntityTypeSymbol,
                    y => StaticAttributeValueDictionaryFactory(y, getDynamoDbProperties),
                    hashset
                )
                .Concat(Conversion.ConversionMethods(x.ArgumentType, y => StaticAttributeValueDictionaryFactory(y, getDynamoDbProperties), hashset))
            )
            .SelectMany(x => x.Code);
    }


    public static IEnumerable<string> CreateRepository(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        var loadedArguments = arguments.ToArray();
        var getDynamoDbProperties = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, static x => x.GetDynamoDbProperties());
        var code = CreateImplementations(loadedArguments)
            .Concat($"private static class {MarshallerClass}".CreateBlock(CreateMarshaller(loadedArguments, getDynamoDbProperties)))
            .Concat($"private static class {UnMarshallerClass}".CreateBlock(CreateUnMarshaller(loadedArguments, getDynamoDbProperties)))
            .Concat(CreateExpressionAttributeName(loadedArguments, getDynamoDbProperties))
            .Concat(CreateExpressionAttributeValue(loadedArguments, getDynamoDbProperties))
            .Concat(CreateKeys(loadedArguments, getDynamoDbProperties));

        return code;
    }

    private static IEnumerable<string> CreateUnMarshaller(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);
        return arguments.SelectMany(x =>
                Conversion.ConversionMethods(
                    x.EntityTypeSymbol,
                    y => StaticPocoFactory(y, getDynamoDbProperties),
                    hashSet
                )
            )
            .SelectMany(x => x.Code);
    }
    private static Conversion ExpressionAttributeName(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        const string constructorAttributeName = "nameRef";
        var dataMembers = fn(typeSymbol)
            .Select(x =>
            {
                var typeIdentifier = GetTypeIdentifier(x.DataMember.Type);
                var nameRef = $"_{x.DataMember.Name}NameRef";
                var attributeReference = GetAttributeExpressionNameTypeName(x.DataMember.Type);
                var isUnknown = typeIdentifier is UnknownType;

                return (
                    IsUnknown: isUnknown,
                    typeIdentifier,
                    DDB: x,
                    NameRef: nameRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: AttributeExpressionNameTrackerInterface
                );
            })
            .ToArray();

        var structName = GetAttributeExpressionNameTypeName(typeSymbol);

        var @class = $"public readonly struct {structName} : {AttributeExpressionNameTrackerInterface}"
            .CreateBlock(CreateCode());
        return new Conversion(@class, dataMembers.Select(x => x.typeIdentifier).OfType<UnknownType>().Select(x => x.TypeSymbol));

        IEnumerable<string> CreateCode()
        {
            const string self = "_self";
            var constructorFieldAssignments = dataMembers
                .Select(x =>
                {
                    var ternaryExpressionName = $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                    return x.IsUnknown
                        ? $"_{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({ternaryExpressionName}));"
                        : $"{x.NameRef} = new (() => {ternaryExpressionName});";
                })
                .Append($@"{self} = new(() => {constructorAttributeName} ?? throw new NotImplementedException(""Root element AttributeExpressionName reference.""));");

            foreach (var fieldAssignment in $"public {structName}(string? {constructorAttributeName})".CreateBlock(constructorFieldAssignments))
                yield return fieldAssignment;

            foreach (var fieldDeclaration in dataMembers)
            {
                if (fieldDeclaration.IsUnknown)
                {
                    yield return $"private readonly Lazy<{fieldDeclaration.AttributeReference}> _{fieldDeclaration.DDB.DataMember.Name};";
                    yield return $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => _{fieldDeclaration.DDB.DataMember.Name}.Value;";
                }
                else
                {
                    yield return $"private readonly Lazy<string> {fieldDeclaration.NameRef};";
                    yield return $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.NameRef}.Value;";

                }
            }
            yield return $"private readonly Lazy<string> {self};";

            var yields = dataMembers
                .Select(static x => x.IsUnknown
                    ? $"if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionNameTrackerInterfaceAccessedNames}()) {{ yield return x; }}"
                    : $@"if ({x.NameRef}.IsValueCreated) yield return new ({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
                )
                .Append($@"if ({self}.IsValueCreated) yield return new ({self}.Value, ""{typeSymbol.Name}"");");

            foreach (var s in $"IEnumerable<KeyValuePair<string, string>> {AttributeExpressionNameTrackerInterface}.{AttributeExpressionNameTrackerInterfaceAccessedNames}()".CreateBlock(yields))
                yield return s;

            yield return $"public override string ToString() => {self}.Value;";
        }

    }

    private static Conversion ExpressionAttributeValue(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        const string valueProvider = "valueIdProvider";
        var dataMembers = fn(typeSymbol)
            .Select(x =>
            {
                var typeIdentifier = GetTypeIdentifier(x.DataMember.Type);
                var valueRef = $"_{x.DataMember.Name}ValueRef";
                var attributeReference = GetAttributeExpressionValueTypeName(x.DataMember.Type);
                var isUnknown = typeIdentifier is UnknownType;

                return (
                    IsUnknown: isUnknown,
                    DDB: x,
                    ValueRef: valueRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: GetAttributeValueInterfaceName(x.DataMember.Type),
                    typeIdentifier
                );
            })
            .ToArray();

        var className = GetAttributeExpressionValueTypeName(typeSymbol);

        var interfaceName = GetAttributeValueInterfaceName(typeSymbol);

        var @struct = $"public readonly struct {className} : {interfaceName}".CreateBlock(CreateCode());

        return new Conversion(@struct, dataMembers.Select(x => x.typeIdentifier).OfType<UnknownType>().Select(x => x.TypeSymbol));

        IEnumerable<string> CreateCode()
        {
            const string self = "_self";
            var constructorFieldAssignments = dataMembers
                .Select(x => x.IsUnknown
                    ? $"_{x.DDB.DataMember.Name} = new (() => new {x.AttributeReference}({valueProvider}));"
                    : $"{x.ValueRef} = new ({valueProvider});")
                .Append($"{self} = new({valueProvider});");
            foreach (var fieldAssignment in $"public {className}(Func<string> {valueProvider})".CreateBlock(constructorFieldAssignments))
                yield return fieldAssignment;

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
                enumerable = $"if ({param} is null)".CreateBlock($"yield return new ({self}.Value, new AttributeValue {{ NULL = true }});","yield break;");
            }else if (typeSymbol.IsReferenceType)
            {
                enumerable = $"if ({param} is null)".CreateBlock($"throw {NullExceptionMethod}(\"{className}\");");
            }


            var yields = enumerable.Concat(
                dataMembers
                    .Select(x =>
                        {
                            var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                            return x.IsUnknown
                                ? $"if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionValueTrackerAccessedValues}({accessPattern})) {{ yield return x; }}")}"
                                : $"if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {InvokeMarshallerMethod(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}", $"\"{x.DDB.DataMember.Name}\"")} ?? new AttributeValue {{ NULL = true }});")}";
                        }
                    )
                    .Append($"if ({self}.IsValueCreated) yield return new ({self}.Value, {InvokeMarshallerMethod(typeSymbol, "entity", $"\"{className}\"")} ?? new AttributeValue {{ NULL = true }});")
            );

            foreach (var yield in $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{AttributeExpressionValueTrackerAccessedValues}({GetTypeName(typeSymbol).annotated} entity)".CreateBlock(yields))
                yield return yield;

            yield return $"public override string ToString() => {self}.Value;";
        }
    }
    private static string InvokeMarshallerMethod(ITypeSymbol typeSymbol, string parameterReference, string dataMember)
    {
        var invocation = $"{MarshallerClass}.{GetSerializationMethodName(typeSymbol)}({parameterReference}, {dataMember})";
        return GetTypeIdentifier(typeSymbol) is UnknownType 
            ? $"{Constants.DynamoDBGenerator.AttributeValueUtilityFactory.ToAttributeValue}({invocation})" 
            : invocation;
    }
    private static string InvokeUnmarshallMethod(ITypeSymbol typeSymbol, string paramReference, string dataMember)
    {
        return GetTypeIdentifier(typeSymbol) is UnknownType
            ? $"{GetDeserializationMethodName(typeSymbol)}({paramReference}?.M, {dataMember})"
            : $"{GetDeserializationMethodName(typeSymbol)}({paramReference}, {dataMember})";

    }
    private static Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        const string param = "entity";
        const string dataMember = "dataMember";

        static string CreateSignature(TypeIdentifier typeIdentifier)
        {
            return typeIdentifier.TypeSymbol.IsNullable()
                ? $"public static AttributeValue? {GetSerializationMethodName(typeIdentifier.TypeSymbol)}({GetTypeName(typeIdentifier.TypeSymbol).annotated} {param}, string? {dataMember} = null)"
                : $"public static AttributeValue {GetSerializationMethodName(typeIdentifier.TypeSymbol)}({GetTypeName(typeIdentifier.TypeSymbol).annotated} {param}, string? {dataMember} = null)";
        }
        

        static string Else(TypeIdentifier typeIdentifier) => typeIdentifier.TypeSymbol.IsNullable() ? "null" : $"throw {NullExceptionMethod}({dataMember})";

        return GetTypeIdentifier(type) switch
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
                _ => throw UncoveredConversionException(baseType, nameof(StaticAttributeValueDictionaryFactory))
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
                        .CreateBlock($"return {param} is not null ? new AttributeValue {{ L = new List<AttributeValue>({param}.Select((y, i) => {InvokeMarshallerMethod(singleGeneric.T, "y", $"$\"{{{dataMember}}}[{{i.ToString()}}]\"")})) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String
                    => signature
                        .CreateBlock($"return {param} is not null ? new AttributeValue {{ SS = new List<string>({param}) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric()
                    => signature
                        .CreateBlock($"return {param} is not null ? new AttributeValue {{ NS = new List<string>({param}.Select(y => y.ToString())) }} : {Else(singleGeneric)};")
                        .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(StaticAttributeValueDictionaryFactory))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(StaticAttributeValueDictionaryFactory))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(StaticAttributeValueDictionaryFactory))),
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
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(StaticAttributeValueDictionaryFactory))
            },
            UnknownType unknownType => CreateDictionaryMethod(unknownType.TypeSymbol),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(StaticAttributeValueDictionaryFactory))

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
            else if(typeSymbol.IsReferenceType)
                enumerable = $"if ({paramReference} is null)".CreateBlock($"throw {NullExceptionMethod}({dataMember});");

            var body =
                enumerable.Concat(InitializeDictionary(properties.Select(x => x.capacityTernary))
                    .Concat(properties.SelectMany(x => x.dictionaryAssignment))
                    .Append($"return {dictionaryReference};"));

            var code =
                $"public static Dictionary<string, AttributeValue>{(isNullable ? '?' : null)} {GetSerializationMethodName(type)}({GetTypeName(type).annotated} {paramReference}, string? {dataMember} = null)".CreateBlock(body);

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

    private static Conversion StaticAttributeValueDictionaryKeys(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        const string pkReference = "partitionKey";
        const string rkReference = "rangeKey";
        const string enforcePkReference = "isPartitionKey";
        const string enforceRkReference = "isRangeKey";
        const string dictionaryName = "attributeValues";

        var code =
            $"private static Dictionary<string, AttributeValue> {GetKeysMethodName(typeSymbol)}(object? {pkReference}, object? {rkReference}, bool {enforcePkReference}, bool {enforceRkReference}, string? index = null)"
                .CreateBlock(CreateBody());

        return new Conversion(code);

        IEnumerable<string> CreateBody()
        {
            var keyStructure = DynamoDbDataMember.GetKeyStructure(fn(typeSymbol));
            if (keyStructure is null)
            {
                yield return @$"throw {NoDynamoDBKeyAttributesExceptionMethod}(""{typeSymbol}"");";

                yield break;
            }

            yield return $"var {dictionaryName} = new Dictionary<string, AttributeValue>(2);";

            var switchBody = GetAssignments(keyStructure.Value)
                .SelectMany(x => $"case {(x.IndexName is null ? "null" : @$"""{x.IndexName}""")}:".CreateBlock(x.assignments).Append("break;"))
                .Append($"default: throw {MissMatchedIndexNameExceptionMethod}(nameof(index), index);");

            foreach (var s in "switch (index)".CreateBlock(switchBody))
                yield return s;

            var validateSwitch = $"if ({enforcePkReference} && {enforceRkReference} && {dictionaryName}.Count == 2)"
                .CreateBlock($"return {dictionaryName};")
                .Concat($"if ({enforcePkReference} && {enforceRkReference} is false && {dictionaryName}.Count == 1)".CreateBlock($"return {dictionaryName};"))
                .Concat($"if ({enforcePkReference} is false && {enforceRkReference} && {dictionaryName}.Count == 1)".CreateBlock($"return {dictionaryName};"))
                .Concat($"if ({enforcePkReference} && {enforceRkReference} && {dictionaryName}.Count == 1)".CreateBlock($"throw {KeysMissingDynamoDBAttributeExceptionMethod}({pkReference}, {rkReference});"))
                .Append($"throw {ShouldNeverHappenExceptionMethod}();");

            foreach (var s in validateSwitch)
                yield return s;

        }

        IEnumerable<(string? IndexName, IEnumerable<string> assignments)> GetAssignments(DynamoDBKeyStructure keyStructure)
        {
            yield return keyStructure switch
            {
                {PartitionKey: var pk, SortKey: { } sortKey} => (null, CreateAssignment(enforcePkReference, pkReference, pk).Concat(CreateAssignment(enforceRkReference, rkReference, sortKey))),
                {PartitionKey: var pk, SortKey: null} => (null, CreateAssignment(enforcePkReference, pkReference, pk).Append(MissingAssigment(enforceRkReference, rkReference)))
            };

            foreach (var gsi in keyStructure.GlobalSecondaryIndices)
            {
                yield return gsi switch
                {
                    {PartitionKey: var pk, SortKey: { } sortKey} => (gsi.Name, CreateAssignment(enforcePkReference, pkReference, pk).Concat(CreateAssignment(enforceRkReference, rkReference, sortKey))),
                    {PartitionKey: var pk, SortKey: null} => (gsi.Name, CreateAssignment(enforcePkReference, pkReference, pk).Append(MissingAssigment(enforceRkReference, rkReference)))
                };
            }

            foreach (var lsi in keyStructure.LocalSecondaryIndices)
            {
                yield return (lsi, keyStructure.PartitionKey) switch
                {
                    {PartitionKey: var pk, lsi: var sortKey} => (lsi.Name, CreateAssignment(enforcePkReference, pkReference, pk).Concat(CreateAssignment(enforceRkReference, rkReference, sortKey.SortKey)))
                };
            }

            string MissingAssigment(string validateReference, string keyReference)
            {
                var expression = $"{validateReference} && {keyReference} is not null";
                return $@"if({expression}) 
                            throw {KeysValueWithNoCorrespondenceMethod}(""{keyReference}"", {keyReference});";
            }

            IEnumerable<string> CreateAssignment(string validateReference, string keyReference, DynamoDbDataMember dataMember)
            {
                const string reference = "value";
                var expectedType = GetTypeName(dataMember.DataMember.Type).original;
                var expression = $"{keyReference} is {expectedType} {{ }} {reference}";

                var innerContent = $"if ({expression}) "
                    .CreateBlock($@"{dictionaryName}.Add(""{dataMember.AttributeName}"", {InvokeMarshallerMethod(dataMember.DataMember.Type, reference, $"nameof({keyReference})")});")
                    .Concat($"else if ({keyReference} is null) ".CreateBlock($@"throw {KeysArgumentNullExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"");"))
                    .Concat("else".CreateBlock($@"throw {KeysInvalidConversionExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"", {keyReference}, ""{expectedType}"");"));

                return $"if({validateReference})".CreateBlock(innerContent);

            }

        }

    }

    private static Conversion StaticPocoFactory(ITypeSymbol type, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {

        const string value = "attributeValue";
        const string dict = "dict";
        const string dataMember = "dataMember";

        static string CreateSignature(TypeIdentifier typeIdentifier)
        {
            return
                $"public static {GetTypeName(typeIdentifier.TypeSymbol).annotated} {GetDeserializationMethodName(typeIdentifier.TypeSymbol)}(AttributeValue? {value}, string? {dataMember} = null)";
        }

        static string Else(TypeIdentifier typeIdentifier) => typeIdentifier.TypeSymbol.IsNullable() ? "null" : $"throw {NullExceptionMethod}({dataMember})";

        return GetTypeIdentifier(type) switch
        {
            BaseType baseType when CreateSignature(baseType) is var signature => baseType.Type switch
            {
                BaseType.SupportedType.String => signature
                    .CreateBlock($"return {value} is {{ S: {{ }} x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Bool => signature
                    .CreateBlock($"return {value} is {{ BOOl: var x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Char => signature
                    .CreateBlock($"return {value} is {{ S: {{ }} x }} ? x[0] : {Else(baseType)};")
                    .ToConversion(),
                BaseType.SupportedType.Enum => signature
                    .CreateBlock($"return {value} is {{ N: {{ }} x }} ? ({GetTypeName(baseType.TypeSymbol).annotated})Int32.Parse(x) : {Else(baseType)};")
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
                        .CreateBlock($"return {value} is {{ N: {{ }} x }} ? {GetTypeName(baseType.TypeSymbol).original}.Parse(x) : {Else(baseType)};")
                        .ToConversion(),
                BaseType.SupportedType.DateTime
                    or BaseType.SupportedType.DateTimeOffset
                    or BaseType.SupportedType.DateOnly
                    => signature
                        .CreateBlock($"return {value} is {{ S: {{ }} x }} ? {GetTypeName(baseType.TypeSymbol).original}.Parse(x) : {Else(baseType)};")
                        .ToConversion(),
                BaseType.SupportedType.MemoryStream => signature
                    .CreateBlock($"return {value} is {{ B: {{ }} x }} ? x : {Else(baseType)};")
                    .ToConversion(),
                _ => throw UncoveredConversionException(baseType, nameof(StaticPocoFactory))
            },
            SingleGeneric singleGeneric when CreateSignature(singleGeneric) is var signature => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => signature
                    .CreateBlock($"return {value} is null or {{ NULL: true }} ? {Else(singleGeneric)} : {InvokeUnmarshallMethod(singleGeneric.T, value, dataMember)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.ICollection => signature
                    .CreateBlock($"return {value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{dataMember}}}[{{i.ToString()}}]\"")}).ToList() : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Array or SingleGeneric.SupportedType.IReadOnlyCollection => signature
                    .CreateBlock($"return {value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{dataMember}}}[{{i.ToString()}}]\"")}).ToArray() : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.IEnumerable => signature
                    .CreateBlock($"return {value} is {{ L: {{ }} x }} ? x.Select((y, i) => {InvokeUnmarshallMethod(singleGeneric.T, "y", $"$\"{{{dataMember}}}[{{i.ToString()}}]\"")}) : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.T),
                SingleGeneric.SupportedType.Set when singleGeneric.T.SpecialType is SpecialType.System_String => signature
                    .CreateBlock($"return {value} is {{ SS : {{ }} x }} ? new HashSet<string>(x) : {Else(singleGeneric)};")
                    .ToConversion(),
                SingleGeneric.SupportedType.Set when singleGeneric.T.IsNumeric() => signature
                    .CreateBlock($"return {value} is {{ NS : {{ }} x }} ? x.Select(y => {GetTypeName(singleGeneric.T).original}.Parse(y)).ToHashSet() : {Else(singleGeneric)};")
                    .ToConversion(singleGeneric.TypeSymbol),
                SingleGeneric.SupportedType.Set => throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(singleGeneric, nameof(StaticPocoFactory))),
                _ => throw UncoveredConversionException(singleGeneric, nameof(StaticPocoFactory))
            },
            KeyValueGeneric {TKey.SpecialType: not SpecialType.System_String} keyValueGeneric => throw new ArgumentException("Only strings are supported for for TKey",
                UncoveredConversionException(keyValueGeneric, nameof(StaticPocoFactory))),
            KeyValueGeneric keyValueGeneric when CreateSignature(keyValueGeneric) is var signature => keyValueGeneric.Type switch
            {
                KeyValueGeneric.SupportedType.Dictionary => signature
                    .CreateBlock($"return {value} is {{ M: {{ }} x }} ? x.ToDictionary(y => y.Key, y => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "y.Value", "y.Key")}) : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                KeyValueGeneric.SupportedType.LookUp => signature
                    .CreateBlock(
                        $"return {value} is {{ M: {{ }} x }} ? x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {InvokeUnmarshallMethod(keyValueGeneric.TValue, "y.z", "y.Key")}) : {Else(keyValueGeneric)};")
                    .ToConversion(keyValueGeneric.TValue),
                _ => throw UncoveredConversionException(keyValueGeneric, nameof(StaticPocoFactory))

            },
            UnknownType => CreateCode(),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(StaticPocoFactory))
        };

        Conversion CreateCode()
        {
            var assignments = fn(type)
                .Select(x => (DDB: x, MethodCall: InvokeUnmarshallMethod(x.DataMember.Type, $"{dict}.GetValueOrDefault(\"{x.AttributeName}\")", $"\"{x.DataMember.Name}\""), x.DataMember.Name))
                .ToArray();

            var typeName = GetTypeName(type);
            var blockBody =
                $"if ({dict} is null)"
                    .CreateBlock(type.IsNullable() ? "return null;" : $"throw {NullExceptionMethod}({dataMember});").Concat(
                        GetAssignments()
                            .DefaultAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
                            .SelectMany(x => x)
                            .DefaultIfEmpty("();")
                            // Is needed in order to not perform new entity? where '?' is not allowed in the end of the string.
                            .Prepend(type.IsTupleType ? "return" : $"return new {(typeName.annotated.EndsWith("?") ? typeName.annotated.Substring(0, typeName.annotated.Length -1) : typeName.annotated)}")
                    );

            var method = $"public static {typeName.annotated} {GetDeserializationMethodName(type)}(Dictionary<string, AttributeValue>? {dict}, string? {dataMember} = null)".CreateBlock(blockBody);

            return new Conversion(method, assignments.Select(x => x.DDB.DataMember.Type));

            static IEnumerable<string> ObjectAssignmentBlock(bool useParentheses, IEnumerable<string> assignments, bool applySemiColon)
            {

                if (useParentheses)
                {
                    yield return "(";

                    foreach (var assignment in assignments.DefaultAndLast(s => $"{s},", s => s))
                        yield return assignment;

                    if (applySemiColon)
                        yield return ");";
                    else
                        yield return ")";
                }
                else
                {
                    yield return "{";

                    foreach (var assignment in assignments.DefaultAndLast(s => $"{s},", s => s))
                        yield return assignment;

                    if (applySemiColon)
                        yield return "};";
                    else
                        yield return "}";

                }

            }

            IEnumerable<(bool useParentheses, IEnumerable<string> assignments)> GetAssignments()
            {
                if (type.IsTupleType)
                    yield return (true, assignments.Select(x => $"{x.DDB.DataMember.Name}: {x.MethodCall}"));
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
                                ? (x.Key, x.Select(z => $"{z.Constructor!.Value.ParameterName} : {z.MethodCall}"))
                                : (x.Key, x.Where(z => z.DDB.DataMember.IsAssignable).Select(z => $"{z.DDB.DataMember.Name} = {z.MethodCall}"));
                        });

                    foreach (var valueTuple in resolve)
                        yield return valueTuple;
                }

            }

            static IEnumerable<(string DataMember, string ParameterName)> TryGetMatchedConstructorArguments(ITypeSymbol typeSymbol)
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
        }
    }

    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }
}