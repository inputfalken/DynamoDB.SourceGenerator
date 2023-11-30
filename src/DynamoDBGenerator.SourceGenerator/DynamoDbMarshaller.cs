using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.ExceptionHelper;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator;

public class DynamoDbMarshaller
{
    private static readonly IEqualityComparer<ISymbol?> Comparer;
    private static readonly Func<ITypeSymbol, string> GetAttributeExpressionNameTypeName;
    private static readonly Func<ITypeSymbol, string> GetAttributeExpressionValueTypeName;
    private static readonly Func<ITypeSymbol, string> GetAttributeValueInterfaceName;
    private static readonly Func<ITypeSymbol, string> GetDeserializationMethodName;
    private static readonly Func<ITypeSymbol, string> GetFullTypeName;
    private static readonly Func<ITypeSymbol, string> GetKeysMethodName;
    private static readonly Func<ITypeSymbol, string> GetSerializationMethodName;
    private static readonly Func<ITypeSymbol, TypeIdentifier> GetTypeIdentifier;
    private const string MarshallerClass = "_Marshaller_";
    private const string UnMarshallerClass = "_Unmarshaller_";

    private readonly IReadOnlyList<DynamoDBMarshallerArguments> _arguments;
    private readonly Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> _cachedDataMembers;

    static DynamoDbMarshaller()
    {
        Comparer = SymbolEqualityComparer.IncludeNullability;
        GetFullTypeName = TypeExtensions.CacheFactory(Comparer, x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        GetTypeIdentifier = TypeExtensions.CacheFactory(Comparer, x => x.GetKnownType());
        GetDeserializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory(null, Comparer, true);
        GetKeysMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("Keys", Comparer, false);
        GetSerializationMethodName = TypeExtensions.SuffixedTypeSymbolNameFactory(null, Comparer, true);
        GetAttributeExpressionNameTypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Names", Comparer, false);
        GetAttributeExpressionValueTypeName = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", Comparer, false);
        GetAttributeValueInterfaceName = TypeExtensions.CacheFactory(Comparer, x => $"{AttributeExpressionValueTrackerInterface}<{GetFullTypeName(x)}>");
    }
    public DynamoDbMarshaller(in IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        _cachedDataMembers = TypeExtensions.CacheFactory(Comparer, static x => x.GetDynamoDbProperties());
        _arguments = arguments.ToArray();
    }

    private IEnumerable<string> CreateExpressionAttributeName()
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return _arguments
            .SelectMany(x => Conversion.ConversionMethods(x.EntityTypeSymbol, ExpressionAttributeName, hashSet)).SelectMany(x => x.Code);

    }
    private IEnumerable<string> CreateExpressionAttributeValue()
    {
        // Using _comparer can double classes when there's a None nullable property mixed with a nullable property
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        return _arguments
            .SelectMany(x => Conversion.ConversionMethods(x.ArgumentType, ExpressionAttributeValue, hashSet)).SelectMany(x => x.Code);
    }
    private IEnumerable<string> CreateImplementations()
    {
        foreach (var argument in _arguments)
        {
            var rootTypeName = GetFullTypeName(argument.EntityTypeSymbol);
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

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{rootTypeName}, {GetFullTypeName(argument.ArgumentType)}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return
                $"public {Interface}<{rootTypeName}, {GetFullTypeName(argument.ArgumentType)}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}();";

            foreach (var s in classImplementation)
                yield return s;

        }
    }
    private IEnumerable<string> CreateKeys()
    {
        var hashSet = new HashSet<ITypeSymbol>(Comparer);

        return _arguments
            .SelectMany(x => Conversion.ConversionMethods(x.EntityTypeSymbol, StaticAttributeValueDictionaryKeys, hashSet)).SelectMany(x => x.Code);
    }

    private IEnumerable<string> CreateMarshaller()
    {
        var hashset = new HashSet<ITypeSymbol>(Comparer);

        return _arguments.SelectMany(x => Conversion
                .ConversionMethods(
                    x.EntityTypeSymbol,
                    StaticAttributeValueDictionaryFactory,
                    hashset
                )
                .Concat(Conversion.ConversionMethods(x.ArgumentType, StaticAttributeValueDictionaryFactory, hashset))
            )
            .SelectMany(x => x.Code);
    }


    public IEnumerable<string> CreateRepository()
    {
        var code = CreateImplementations()
            .Concat($"private static class {MarshallerClass}".CreateBlock(CreateMarshaller()))
            .Concat($"private static class {UnMarshallerClass}".CreateBlock(CreateUnMarshaller()))
            .Concat(CreateExpressionAttributeName())
            .Concat(CreateExpressionAttributeValue())
            .Concat(CreateKeys());

        return code;
    }

    private IEnumerable<string> CreateUnMarshaller()
    {
        var hashSet = new HashSet<ITypeSymbol>(Comparer);
        return _arguments.SelectMany(x =>
                Conversion.ConversionMethods(
                    x.EntityTypeSymbol,
                    StaticPocoFactory,
                    hashSet
                )
            )
            .SelectMany(x => x.Code);
    }
    private Conversion ExpressionAttributeName(ITypeSymbol typeSymbol)
    {
        const string constructorAttributeName = "nameRef";
        var dataMembers = _cachedDataMembers(typeSymbol)
            .Select(x =>
            {
                var ternaryExpressionName = $"{constructorAttributeName} is null ? {@$"""#{x.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.AttributeName}"""}";
                var typeIdentifier = GetTypeIdentifier(x.DataMember.Type);
                var nameRef = $"_{x.DataMember.Name}NameRef";
                var attributeReference = GetAttributeExpressionNameTypeName(x.DataMember.Type);
                var isUnknown = typeIdentifier is UnknownType;

                var assignment = isUnknown
                    ? $"_{x.DataMember.Name} = new (() => new {attributeReference}({ternaryExpressionName}));"
                    : $"{nameRef} = new (() => {ternaryExpressionName});";

                return (
                    IsUnknown: isUnknown,
                    DDB: x,
                    NameRef: nameRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: AttributeExpressionNameTrackerInterface,
                    Assignment: new Assignment(assignment, typeIdentifier)
                );
            })
            .ToArray();

        var structName = GetAttributeExpressionNameTypeName(typeSymbol);

        var @class = $"public readonly struct {structName} : {AttributeExpressionNameTrackerInterface}"
            .CreateBlock(CreateCode());
        return new Conversion(@class, dataMembers.Select(x => x.Assignment));

        IEnumerable<string> CreateCode()
        {
            const string self = "_self";
            var constructorFieldAssignments = dataMembers
                .Select(x => x.Assignment.Value)
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

    private Conversion ExpressionAttributeValue(ITypeSymbol typeSymbol)
    {
        const string valueProvider = "valueIdProvider";
        var dataMembers = _cachedDataMembers(typeSymbol)
            .Select(x =>
            {
                var typeIdentifier = GetTypeIdentifier(x.DataMember.Type);
                var valueRef = $"_{x.DataMember.Name}ValueRef";
                var attributeReference = GetAttributeExpressionValueTypeName(x.DataMember.Type);
                var isUnknown = typeIdentifier is UnknownType;
                var assignment = isUnknown
                    ? $"_{x.DataMember.Name} = new (() => new {attributeReference}({valueProvider}));"
                    : $"{valueRef} = new ({valueProvider});";

                return (
                    IsUnknown: isUnknown,
                    DDB: x,
                    ValueRef: valueRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: GetAttributeValueInterfaceName(x.DataMember.Type),
                    Assignment: new Assignment(assignment, typeIdentifier)
                );
            })
            .ToArray();

        var className = GetAttributeExpressionValueTypeName(typeSymbol);

        var interfaceName = GetAttributeValueInterfaceName(typeSymbol);

        var @struct = $"public readonly struct {className} : {interfaceName}".CreateBlock(CreateCode());

        return new Conversion(@struct, dataMembers.Select(x => x.Assignment));

        IEnumerable<string> CreateCode()
        {
            const string self = "_self";
            var constructorFieldAssignments = dataMembers.Select(x => x.Assignment.Value).Append($"{self} = new({valueProvider});");
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

            var yields = dataMembers
                .Select(x =>
                    {
                        var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                        return x.IsUnknown
                            ? $"if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionValueTrackerAccessedValues}({accessPattern})) {{ yield return x; }}")}"
                            : $"if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {MarshallingAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}";
                    }
                )
                .Append($"if ({self}.IsValueCreated) yield return new ({self}.Value, {MarshallingAssignment(typeSymbol, "entity").ToAttributeValue()});");

            foreach (var yield in $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{AttributeExpressionValueTrackerAccessedValues}({GetFullTypeName(typeSymbol)} entity)".CreateBlock(yields))
                yield return yield;

            yield return $"public override string ToString() => {self}.Value;";
        }
    }
    private Assignment MarshallingAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        return GetTypeIdentifier(typeSymbol) switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => baseType.ToInlineAssignment($"S = {accessPattern}"),
                BaseType.SupportedType.Bool => baseType.ToInlineAssignment($"BOOL = {accessPattern}"),
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
                    => baseType.ToInlineAssignment($"N = {accessPattern}.ToString()"),
                BaseType.SupportedType.Char => baseType.ToInlineAssignment($"S = {accessPattern}.ToString()"),
                BaseType.SupportedType.DateOnly or BaseType.SupportedType.DateTimeOffset or BaseType.SupportedType.DateTime => baseType.ToInlineAssignment($@"S = {accessPattern}.ToString(""O"")"),
                BaseType.SupportedType.Enum => baseType.ToInlineAssignment($"N = ((int){accessPattern}).ToString()"),
                BaseType.SupportedType.MemoryStream => baseType.ToInlineAssignment($"B = {accessPattern}"),
                _ => throw UncoveredConversionException(baseType, nameof(MarshallingAssignment))
            },
            UnknownType unknownType => unknownType.ToExternalDependencyAssignment($"M = {MarshallerClass}.{GetSerializationMethodName(unknownType.TypeSymbol)}({accessPattern})"),
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => MarshallingAssignment(singleGeneric.T, $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => MarshallList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Set => MarshallSet(singleGeneric, accessPattern),
                _ => throw UncoveredConversionException(singleGeneric, nameof(MarshallingAssignment))
            },
            KeyValueGeneric keyValueGeneric => MarshallKeyValue(in keyValueGeneric, in accessPattern),
            var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(MarshallingAssignment))
        };

        static Assignment MarshallSet(in SingleGeneric typeIdentifier, in string ap)
        {
            var newAccessPattern = typeIdentifier.T.NotNullLambdaExpression() is { } expression
                ? $"{ap}.Where({expression})"
                : ap;

            if (typeIdentifier.T.SpecialType is SpecialType.System_String)
                return typeIdentifier.ToInlineAssignment($"SS = new List<string>({newAccessPattern})");

            return typeIdentifier.T.IsNumeric()
                ? typeIdentifier.ToInlineAssignment($"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))")
                : throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(typeIdentifier, nameof(MarshallSet)));

        }

        Assignment MarshallList(in ITypeSymbol typeSymbol, in string ap)
        {
            var innerAssignment = MarshallingAssignment(typeSymbol, "x");
            var select = $"Select(x => {innerAssignment.ToAttributeValue()}))";
            var outerAssignment = typeSymbol.NotNullLambdaExpression() is { } whereBody
                ? $"L = new List<AttributeValue>({ap}.Where({whereBody}).{select}"
                : $"L = new List<AttributeValue>({ap}.{select}";

            return new Assignment(outerAssignment, innerAssignment.TypeIdentifier);
        }

        Assignment MarshallKeyValue(in KeyValueGeneric keyValueGeneric, in string ap)
        {
            switch (keyValueGeneric)
            {
                case {TKey: not {SpecialType: SpecialType.System_String}}:
                    throw new ArgumentException("Only strings are supported for for TKey", UncoveredConversionException(keyValueGeneric, nameof(MarshallKeyValue)));
                case {Type: KeyValueGeneric.SupportedType.LookUp}:
                    var lookupValueAssignment = MarshallList(keyValueGeneric.TValue, "x");
                    return new Assignment(
                        $"M = {ap}.ToDictionary(x => x.Key, x => {lookupValueAssignment.ToAttributeValue()})",
                        lookupValueAssignment.TypeIdentifier
                    );
                case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                    var dictionaryValueAssignment = MarshallingAssignment(keyValueGeneric.TValue, "x.Value");
                    return new Assignment(
                        $"M = {ap}.ToDictionary(x => x.Key, x => {dictionaryValueAssignment.ToAttributeValue()})",
                        dictionaryValueAssignment.TypeIdentifier
                    );
                default:
                    throw UncoveredConversionException(keyValueGeneric, nameof(MarshallingAssignment));
            }
        }
    }
    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type).ToArray();

        var body = InitializeDictionary(properties.Select(static x => x.capacityTernary))
            .Concat(properties.Select(x => x.dictionaryPopulation))
            .Append($"return {dictionaryName};");

        var code = $"public static Dictionary<string, AttributeValue> {GetSerializationMethodName(type)}({GetFullTypeName(type)} {paramReference})".CreateBlock(body);

        return new Conversion(code, properties.Select(static x => x.assignment));

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(ITypeSymbol typeSymbol)
        {
            foreach (var x in _cachedDataMembers(typeSymbol))
            {
                var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                var attributeValue = MarshallingAssignment(x.DataMember.Type, accessPattern);

                var dictionaryAssignment = x.DataMember.Type.NotNullIfStatement(
                    in accessPattern,
                    @$"{dictionaryName}.Add(""{x.AttributeName}"", {attributeValue.ToAttributeValue()});"
                );

                var capacityTernary = x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0");

                yield return ($"{dictionaryAssignment}", capacityTernary, attributeValue);
            }
        }

        static IEnumerable<string> InitializeDictionary(IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);
            if (capacityCalculation is "")
            {
                yield return $"var {dictionaryName} = new Dictionary<string, AttributeValue>(0);";
            }
            else
            {
                yield return $"var capacity = {capacityCalculation};";
                yield return $"var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity);";
            }
        }
    }

    private Conversion StaticAttributeValueDictionaryKeys(ITypeSymbol typeSymbol)
    {
        const string pkReference = "partitionKey";
        const string rkReference = "rangeKey";
        const string enforcePkReference = "isPartitionKey";
        const string enforceRkReference = "isRangeKey";
        const string dictionaryName = "attributeValues";

        var code =
            $"private static Dictionary<string, AttributeValue> {GetKeysMethodName(typeSymbol)}(object? {pkReference}, object? {rkReference}, bool {enforcePkReference}, bool {enforceRkReference}, string? index = null)"
                .CreateBlock(CreateBody());
        return new Conversion(code, Enumerable.Empty<Assignment>());

        IEnumerable<string> CreateBody()
        {
            var keyStructure = DynamoDbDataMember.GetKeyStructure(_cachedDataMembers(typeSymbol));
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
                var attributeConversion = MarshallingAssignment(dataMember.DataMember.Type, reference);
                var expectedType = GetFullTypeName(dataMember.DataMember.Type);
                var expression = $"{keyReference} is {expectedType} {{ }} {reference}";

                var innerContent = $"if ({expression}) "
                    .CreateBlock($@"{dictionaryName}.Add(""{dataMember.AttributeName}"", {attributeConversion.ToAttributeValue()});")
                    .Concat($"else if ({keyReference} is null) ".CreateBlock($@"throw {KeysArgumentNullExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"");"))
                    .Concat("else".CreateBlock($@"throw {KeysInvalidConversionExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"", {keyReference}, ""{expectedType}"");"));

                return $"if({validateReference})".CreateBlock(innerContent);

            }

        }

    }

    private Conversion StaticPocoFactory(ITypeSymbol type)
    {

        const string paramReference = "attributeValues";
        var assignments = _cachedDataMembers(type)
            .Select(x => (DDB: x, Assignment: UnmarshallingAssignment(x.DataMember.Type, @$"{paramReference}.GetValueOrDefault(""{x.AttributeName}"")", x.DataMember.Name)))
            .ToArray();

        var blockBody = GetAssignments()
            .DefaultAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
            .SelectMany(x => x)
            .DefaultIfEmpty("();")
            .Prepend(type.IsTupleType ? "return" : $"return new {GetFullTypeName(type)}");

        var method = $"public static {GetFullTypeName(type)} {GetDeserializationMethodName(type)}(Dictionary<string, AttributeValue> {paramReference})".CreateBlock(blockBody);

        return new Conversion(method, assignments.Select(x => x.Assignment));

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
                yield return (true, assignments.Select(x => $"{x.DDB.DataMember.Name}: {x.Assignment.Value}"));
            else
            {
                var resolve = assignments
                    .GroupJoin(
                        TryGetMatchedConstructorArguments(type),
                        x => x.DDB.DataMember.Name,
                        x => x.DataMember,
                        (x, y) => (x.Assignment, x.DDB, Constructor: y.OfType<(string DataMember, string ParameterName)?>().FirstOrDefault())
                    )
                    .GroupBy(x => x.Constructor.HasValue)
                    .OrderByDescending(x => x.Key)
                    .Select(x =>
                    {
                        return x.Key
                            ? (x.Key, x.Select(z => $"{z.Constructor!.Value.ParameterName} : {z.Assignment.Value}"))
                            : (x.Key, x.Where(z => z.DDB.DataMember.IsAssignable).Select(z => $"{z.DDB.DataMember.Name} = {z.Assignment.Value}"));
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

    private static ArgumentException UncoveredConversionException(TypeIdentifier typeIdentifier, string method)
    {
        return new ArgumentException($"The '{typeIdentifier.GetType().FullName}' with backing type '{typeIdentifier.TypeSymbol.ToDisplayString()}' has not been covered in method '{method}'.");
    }

    private Assignment UnmarshallingAssignment(in ITypeSymbol type, in string pattern, in string memberName)
    {

        var defaultCase = type.IsNullable() ? "_ => null" : @$"_ => throw {NullExceptionMethod}(""{memberName}"")";
        return Execution(in type, in pattern, defaultCase, in memberName);

        Assignment Execution(in ITypeSymbol typeSymbol, in string accessPattern, string @default, in string memberName)
        {
            return GetTypeIdentifier(typeSymbol) switch
            {
                BaseType baseType => baseType.Type switch
                {
                    BaseType.SupportedType.String => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x, {@default} }}"),
                    BaseType.SupportedType.Bool => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ BOOL: var x }} => x, {@default} }}"),
                    BaseType.SupportedType.Char => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x[0], {@default} }}"),
                    BaseType.SupportedType.Enum => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} when Int32.Parse(x) is var y =>({GetFullTypeName(typeSymbol)})y, {@default} }}"),
                    BaseType.SupportedType.Int16 => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int16.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Byte => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Byte.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Int32 => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int32.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Int64 => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int64.Parse(x), {@default} }}"),
                    BaseType.SupportedType.SByte => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => SByte.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt16 => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt16.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt32 => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt32.Parse(x), {@default} }}"),
                    BaseType.SupportedType.UInt64 => baseType.ToInlineAssignment($"{accessPattern} switch {{  {{ N: {{ }} x }} => UInt64.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Decimal => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Decimal.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Double => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Double.Parse(x), {@default} }}"),
                    BaseType.SupportedType.Single => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Single.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateTime => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTime.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateTimeOffset => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTimeOffset.Parse(x), {@default} }}"),
                    BaseType.SupportedType.DateOnly => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateOnly.Parse(x), {@default} }}"),
                    BaseType.SupportedType.MemoryStream => baseType.ToInlineAssignment($"{accessPattern} switch {{ {{ B: {{ }} x }} => x, {@default} }}"),
                    _ => throw UncoveredConversionException(baseType, nameof(UnmarshallingAssignment))
                },
                UnknownType x => x.ToExternalDependencyAssignment($"{accessPattern} switch {{ {{ M: {{ }} x }} => {UnMarshallerClass}.{GetDeserializationMethodName(x.TypeSymbol)}(x), {@default} }}"),
                SingleGeneric singleGeneric => singleGeneric.Type switch
                {
                    SingleGeneric.SupportedType.Nullable => Execution(singleGeneric.T, in accessPattern, @default, in memberName),
                    SingleGeneric.SupportedType.Set => UnmarshallSet(singleGeneric, accessPattern, @default),
                    SingleGeneric.SupportedType.Array => UnMarshalList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.ICollection => UnMarshalList(in singleGeneric, ".ToList()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IReadOnlyCollection => UnMarshalList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IEnumerable => UnMarshalList(in singleGeneric, null, in accessPattern, in @default, in memberName),
                    _ => throw UncoveredConversionException(singleGeneric, nameof(UnmarshallingAssignment))
                },
                KeyValueGeneric keyValueGeneric => UnmarshallKeyValue(in keyValueGeneric, in accessPattern, in @default, in memberName),
                var typeIdentifier => throw UncoveredConversionException(typeIdentifier, nameof(UnmarshallingAssignment))
            };

        }

        static Assignment UnmarshallSet(in SingleGeneric typeIdentifier, in string ap, in string dc)
        {
            if (typeIdentifier.T.IsNumeric())
                return typeIdentifier.ToInlineAssignment($"{ap} switch {{ {{ NS: {{ }} x }} =>  new HashSet<{typeIdentifier.T.Name}>(x.Select(y => {typeIdentifier.T.Name}.Parse(y))), {dc} }}");

            if (typeIdentifier.T.SpecialType is SpecialType.System_String)
                return typeIdentifier.ToInlineAssignment($"{ap} switch {{ {{ SS: {{ }} x }} =>  new HashSet<string>(x), {dc} }}");

            throw new ArgumentException("Only string and integers are supported for sets", UncoveredConversionException(typeIdentifier, nameof(UnmarshallSet)));
        }

        Assignment UnMarshalList(in SingleGeneric singleGeneric, in string? operation, in string ap, in string dc, in string mn)
        {
            var innerAssignment = UnmarshallingAssignment(singleGeneric.T, "y", in mn);
            var outerAssignment = $"{ap} switch {{ {{ L: {{ }} x }} => x.Select(y => {innerAssignment.Value}){operation}, {dc} }}";

            return new Assignment(outerAssignment, innerAssignment.TypeIdentifier);
        }

        Assignment UnmarshallKeyValue(in KeyValueGeneric keyValueGeneric, in string ap, in string dc, in string mn)
        {
            switch (keyValueGeneric)
            {
                case {TKey: not {SpecialType: SpecialType.System_String}}:
                    throw new ArgumentException("Only strings are supported for for TKey", UncoveredConversionException(keyValueGeneric, nameof(UnmarshallKeyValue)));
                case {Type: KeyValueGeneric.SupportedType.LookUp}:
                    var lookupValueAssignment = UnmarshallingAssignment(keyValueGeneric.TValue, "y.z", in mn);
                    return new Assignment(
                        $"{ap} switch {{ {{ M: {{ }} x }} => x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {lookupValueAssignment.Value}), {dc} }}",
                        lookupValueAssignment.TypeIdentifier
                    );
                case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                    var dictionaryValueAssignment = UnmarshallingAssignment(keyValueGeneric.TValue, "y.Value", in mn);
                    return new Assignment(
                        $"{ap} switch {{ {{ M: {{ }} x }} => x.ToDictionary(y => y.Key, y => {dictionaryValueAssignment.Value}), {dc} }}",
                        dictionaryValueAssignment.TypeIdentifier
                    );
                default:
                    throw UncoveredConversionException(keyValueGeneric, nameof(UnmarshallingAssignment));
            }
        }

    }
}