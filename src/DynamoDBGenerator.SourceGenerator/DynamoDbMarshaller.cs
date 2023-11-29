using System;
using System.Collections.Generic;
using System.Linq;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.ExceptionHelper;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator;

public class DynamoDbMarshaller
{
    private static readonly Func<ITypeSymbol, string> AttributeNameAssignmentNameFactory;
    private static readonly Func<ITypeSymbol, string> AttributeValueAssignmentNameFactory;
    private static readonly Func<ITypeSymbol, string> AttributeValueInterfaceNameFactory;

    private static readonly IEqualityComparer<ISymbol?> Comparer;
    private static readonly Func<ITypeSymbol, string> DeserializationMethodNameFactory;
    private static readonly Func<ITypeSymbol, string> FullTypeNameFactory;
    private static readonly Func<ITypeSymbol, string> KeysMethodNameFactory;
    private static readonly Func<ITypeSymbol, KnownType?> KnownTypeFactory;
    private const string MarshallerClass = "_Marshaller_";
    private static readonly Func<ITypeSymbol, string> SerializationMethodNameFactory;
    private const string UnMarshallerClass = "_Unmarshaller_";
    private readonly IReadOnlyList<DynamoDBMarshallerArguments> _arguments;

    private readonly Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> _cachedDataMembers;

    static DynamoDbMarshaller()
    {
        Comparer = SymbolEqualityComparer.IncludeNullability;
        FullTypeNameFactory = TypeExtensions.CacheFactory(Comparer, static x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        KnownTypeFactory = TypeExtensions.CacheFactory(Comparer, static x => x.GetKnownType());
        DeserializationMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory(null, Comparer, true);
        KeysMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("Keys", Comparer, false);
        SerializationMethodNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory(null, Comparer, true);
        AttributeNameAssignmentNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("Names", Comparer, false);
        AttributeValueAssignmentNameFactory = TypeExtensions.SuffixedTypeSymbolNameFactory("Values", Comparer, false);
        AttributeValueInterfaceNameFactory = TypeExtensions.CacheFactory(Comparer, x => $"{AttributeExpressionValueTrackerInterface}<{FullTypeNameFactory(x)}>");
    }
    public DynamoDbMarshaller(in IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        _cachedDataMembers = TypeExtensions.CacheFactory(Comparer, static x => x.GetDynamoDbProperties());
        _arguments = arguments.ToArray();
    }
    private Assignment AttributeValueAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        if (KnownTypeFactory(typeSymbol) is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

        var assignment = knownType switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"S = {accessPattern}", knownType),
                BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"BOOL = {accessPattern}", knownType),
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
                    => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()", knownType),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()", knownType),
                BaseType.SupportedType.DateOnly or BaseType.SupportedType.DateTimeOffset or BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($@"S = {accessPattern}.ToString(""O"")", knownType),
                BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"N = ((int){accessPattern}).ToString()", knownType),
                BaseType.SupportedType.MemoryStream => typeSymbol.ToInlineAssignment($"B = {accessPattern}", knownType),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Nullable => AttributeValueAssignment(singleGeneric.T, $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.IReadOnlyCollection
                    or SingleGeneric.SupportedType.Array
                    or SingleGeneric.SupportedType.IEnumerable
                    or SingleGeneric.SupportedType.ICollection => BuildList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Set => BuildSet(singleGeneric.T, accessPattern, knownType),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            KeyValueGeneric keyValueGeneric => StringKeyedValuedGeneric(in keyValueGeneric, in accessPattern),
            _ => null
        };

        return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

        Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
            typeSymbol.ToExternalDependencyAssignment($"M = {MarshallerClass}.{SerializationMethodNameFactory(typeSymbol)}({accessPattern})");
    }

    private Assignment BuildList(in ITypeSymbol elementType, in string accessPattern)
    {
        var innerAssignment = AttributeValueAssignment(elementType, "x");
        var select = $"Select(x => {innerAssignment.ToAttributeValue()}))";
        var outerAssignment = elementType.NotNullLambdaExpression() is { } whereBody
            ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
            : $"L = new List<AttributeValue>({accessPattern}.{select}";

        return new Assignment(in outerAssignment, in elementType, innerAssignment.KnownType);
    }

    private Assignment BuildPocoList(in SingleGeneric singleGeneric, in string? operation, in string accessPattern, in string defaultCause, in string memberName)
    {
        var innerAssignment = DataMemberAssignment(singleGeneric.T, "y", in memberName);
        var outerAssignment = $"{accessPattern} switch {{ {{ L: {{ }} x }} => x.Select(y => {innerAssignment.Value}){operation}, {defaultCause} }}";

        return new Assignment(in outerAssignment, singleGeneric.T, innerAssignment.KnownType);
    }

    private static Assignment? BuildPocoSet(in ITypeSymbol elementType, in string accessPattern, in string defaultCase, KnownType knownType)
    {
        if (elementType.IsNumeric())
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ NS: {{ }} x }} =>  new HashSet<{elementType.Name}>(x.Select(y => {elementType.Name}.Parse(y))), {defaultCase} }}", knownType);

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"{accessPattern} switch {{ {{ SS: {{ }} x }} =>  new HashSet<string>(x), {defaultCase} }}", knownType);

        return null;
    }

    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern, KnownType knownType)
    {
        var newAccessPattern = elementType.NotNullLambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return elementType.ToInlineAssignment($"SS = new List<string>({newAccessPattern})", knownType);

        return elementType.IsNumeric() is false
            ? null
            : elementType.ToInlineAssignment($"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))", knownType);
    }

    private IEnumerable<string> CreateUnmarshaller()
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
            var rootTypeName = FullTypeNameFactory(argument.EntityTypeSymbol);
            var valueTrackerTypeName = AttributeValueAssignmentNameFactory(argument.ArgumentType);
            var nameTrackerTypeName = AttributeNameAssignmentNameFactory(argument.EntityTypeSymbol);

            var interfaceImplementation = $"public Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> {MarshallMethodName}({rootTypeName} entity)"
                .CreateBlock(
                    "ArgumentNullException.ThrowIfNull(entity);",
                    $"return {MarshallerClass}.{SerializationMethodNameFactory(argument.EntityTypeSymbol)}(entity);"
                )
                .Concat($"public {rootTypeName} {UnmarshalMethodName}(Dictionary<{nameof(String)}, {Constants.AWSSDK_DynamoDBv2.AttributeValue}> entity)".CreateBlock(
                    "ArgumentNullException.ThrowIfNull(entity);",
                    $"return {UnMarshallerClass}.{DeserializationMethodNameFactory(argument.EntityTypeSymbol)}(entity);")
                )
                .Concat($"public {IndexKeyMarshallerInterface} IndexKeyMarshaller(string index)".CreateBlock(
                        "ArgumentNullException.ThrowIfNull(index);",
                        $"return new {Constants.DynamoDBGenerator.IndexKeyMarshallerImplementationTypeName}({KeysMethodNameFactory(argument.EntityTypeSymbol)}, index);"
                    )
                )
                .Concat($"public {valueTrackerTypeName} {AttributeExpressionValueTrackerMethodName}()".CreateBlock(
                        "var incrementer = new DynamoExpressionValueIncrementer();",
                        $"return new {valueTrackerTypeName}(incrementer.GetNext);"
                    )
                )
                .Append($"public {nameTrackerTypeName} {AttributeExpressionNameTrackerMethodName}() => new {nameTrackerTypeName}(null);")
                .Append($"public {KeyMarshallerInterface} PrimaryKeyMarshaller {{ get; }} = new {Constants.DynamoDBGenerator.KeyMarshallerImplementationTypeName}({KeysMethodNameFactory(argument.EntityTypeSymbol)});");

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{rootTypeName}, {FullTypeNameFactory(argument.ArgumentType)}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return
                $"public {Interface}<{rootTypeName}, {FullTypeNameFactory(argument.ArgumentType)}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}();";

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


    public IEnumerable<string> CreateRepository()
    {
        var code = CreateImplementations()
            .Concat($"private static class {MarshallerClass}".CreateBlock(CreateMarshaller()))
            .Concat($"private static class {UnMarshallerClass}".CreateBlock(CreateUnmarshaller()))
            .Concat(CreateExpressionAttributeName())
            .Concat(CreateExpressionAttributeValue())
            .Concat(CreateKeys());

        return code;
    }

    private Assignment DataMemberAssignment(in ITypeSymbol type, in string pattern, in string memberName)
    {

        var defaultCase = type.IsNullable() ? "_ => null" : @$"_ => throw {NullExceptionMethod}(""{memberName}"")";
        return Execution(in type, in pattern, defaultCase, in memberName);

        Assignment Execution(in ITypeSymbol typeSymbol, in string accessPattern, string @default, in string memberName)
        {
            if (KnownTypeFactory(typeSymbol) is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

            var assignment = knownType switch
            {
                BaseType baseType => baseType.Type switch
                {
                    BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x, {@default} }}", knownType),
                    BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ BOOL: var x }} => x, {@default} }}", knownType),
                    BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => x[0], {@default} }}", knownType),
                    BaseType.SupportedType.Enum => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} when Int32.Parse(x) is var y =>({FullTypeNameFactory(typeSymbol)})y, {@default} }}", knownType),
                    BaseType.SupportedType.Int16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int16.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Byte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Byte.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Int32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int32.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Int64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Int64.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.SByte => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => SByte.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt16 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt16.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt32 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => UInt32.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.UInt64 => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{  {{ N: {{ }} x }} => UInt64.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Decimal => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Decimal.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Double => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Double.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.Single => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ N: {{ }} x }} => Single.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateTime => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTime.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateTimeOffset => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateTimeOffset.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.DateOnly => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ S: {{ }} x }} => DateOnly.Parse(x), {@default} }}", knownType),
                    BaseType.SupportedType.MemoryStream => typeSymbol.ToInlineAssignment($"{accessPattern} switch {{ {{ B: {{ }} x }} => x, {@default} }}", knownType),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                SingleGeneric singleGeneric => singleGeneric.Type switch
                {
                    SingleGeneric.SupportedType.Nullable => Execution(singleGeneric.T, in accessPattern, @default, in memberName),
                    SingleGeneric.SupportedType.Set => BuildPocoSet(singleGeneric.T, in accessPattern, in @default, knownType),
                    SingleGeneric.SupportedType.Array => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.ICollection => BuildPocoList(in singleGeneric, ".ToList()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IReadOnlyCollection => BuildPocoList(in singleGeneric, ".ToArray()", in accessPattern, in @default, in memberName),
                    SingleGeneric.SupportedType.IEnumerable => BuildPocoList(in singleGeneric, null, in accessPattern, in @default, in memberName),
                    _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
                },
                KeyValueGeneric keyValueGeneric => StringKeyedPocoGeneric(in keyValueGeneric, in accessPattern, in @default, in memberName),
                _ => null
            };

            return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

            Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
                typeSymbol.ToExternalDependencyAssignment($"{accessPattern} switch {{ {{ M: {{ }} x }} => {UnMarshallerClass}.{DeserializationMethodNameFactory(typeSymbol)}(x), {@default} }}");
        }

    }
    private Conversion ExpressionAttributeName(ITypeSymbol typeSymbol)
    {
        const string constructorAttributeName = "nameRef";
        var dataMembers = _cachedDataMembers(typeSymbol)
            .Select(x =>
            {
                var ternaryExpressionName = $"{constructorAttributeName} is null ? {@$"""#{x.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.AttributeName}"""}";
                var knownType = KnownTypeFactory(x.DataMember.Type);
                var nameRef = $"_{x.DataMember.Name}NameRef";
                var attributeReference = AttributeNameAssignmentNameFactory(x.DataMember.Type);

                var assignment = knownType is not null
                    ? $"{nameRef} = new (() => {ternaryExpressionName});"
                    : $"_{x.DataMember.Name} = new (() => new {attributeReference}({ternaryExpressionName}));";

                return (
                    KnownType: knownType,
                    DDB: x,
                    NameRef: nameRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: AttributeExpressionNameTrackerInterface,
                    Assignment: new Assignment(assignment, x.DataMember.Type, knownType)
                );
            })
            .ToArray();

        var structName = AttributeNameAssignmentNameFactory(typeSymbol);

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
                if (fieldDeclaration.KnownType is not null)
                {
                    yield return $"private readonly Lazy<string> {fieldDeclaration.NameRef};";
                    yield return $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.NameRef}.Value;";
                }
                else
                {
                    yield return $"private readonly Lazy<{fieldDeclaration.AttributeReference}> _{fieldDeclaration.DDB.DataMember.Name};";
                    yield return $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => _{fieldDeclaration.DDB.DataMember.Name}.Value;";
                }
            }
            yield return $"private readonly Lazy<string> {self};";

            var yields = dataMembers
                .Select(static x => x.KnownType is not null
                    ? $@"if ({x.NameRef}.IsValueCreated) yield return new ({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
                    : $"if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionNameTrackerInterfaceAccessedNames}()) {{ yield return x; }}"
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
                var knownType = KnownTypeFactory(x.DataMember.Type);
                var valueRef = $"_{x.DataMember.Name}ValueRef";
                var attributeReference = AttributeValueAssignmentNameFactory(x.DataMember.Type);
                var assignment = knownType is not null
                    ? $"{valueRef} = new ({valueProvider});"
                    : $"_{x.DataMember.Name} = new (() => new {attributeReference}({valueProvider}));";
                
                return (
                    KnownType: knownType,
                    DDB: x,
                    ValueRef: valueRef,
                    AttributeReference: attributeReference,
                    AttributeInterfaceName: AttributeValueInterfaceNameFactory(x.DataMember.Type),
                    Assignment: new Assignment(assignment, x.DataMember.Type, knownType)
                );
            })
            .ToArray();

        var className = AttributeValueAssignmentNameFactory(typeSymbol);

        var interfaceName = AttributeValueInterfaceNameFactory(typeSymbol);

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
                if (fieldDeclaration.KnownType is not null)
                {
                    yield return $"private readonly Lazy<string> {fieldDeclaration.ValueRef};";
                    yield return $"public string {fieldDeclaration.DDB.DataMember.Name} => {fieldDeclaration.ValueRef}.Value;";
                }
                else
                {
                    yield return $"private readonly Lazy<{fieldDeclaration.AttributeReference}> _{fieldDeclaration.DDB.DataMember.Name};";
                    yield return $"public {fieldDeclaration.AttributeReference} {fieldDeclaration.DDB.DataMember.Name} => _{fieldDeclaration.DDB.DataMember.Name}.Value;";
                }
            }
            yield return $"private readonly Lazy<string> {self};";

            var yields = dataMembers
                .Select(x =>
                    {
                        var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                        return x.KnownType is not null
                            ? $"if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new ({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                            : $"if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {x.AttributeInterfaceName}).{AttributeExpressionValueTrackerAccessedValues}({accessPattern})) {{ yield return x; }}")}";
                    }
                )
                .Append($"if ({self}.IsValueCreated) yield return new ({self}.Value, {AttributeValueAssignment(typeSymbol, "entity").ToAttributeValue()});");

            foreach (var yield in $"IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{AttributeExpressionValueTrackerAccessedValues}({FullTypeNameFactory(typeSymbol)} entity)".CreateBlock(yields))
                yield return yield;

            yield return $"public override string ToString() => {self}.Value;";
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

        var code = $"public static Dictionary<string, AttributeValue> {SerializationMethodNameFactory(type)}({FullTypeNameFactory(type)} {paramReference})".CreateBlock(body);

        return new Conversion(code, properties.Select(static x => x.assignment));

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(ITypeSymbol typeSymbol)
        {
            foreach (var x in _cachedDataMembers(typeSymbol))
            {
                var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                var attributeValue = AttributeValueAssignment(x.DataMember.Type, accessPattern);

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
            $"private static Dictionary<string, AttributeValue> {KeysMethodNameFactory(typeSymbol)}(object? {pkReference}, object? {rkReference}, bool {enforcePkReference}, bool {enforceRkReference}, string? index = null)"
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
                var attributeConversion = AttributeValueAssignment(dataMember.DataMember.Type, reference);
                var expectedType = FullTypeNameFactory(dataMember.DataMember.Type);
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
            .Select(x => (DDB: x, Assignment: DataMemberAssignment(x.DataMember.Type, @$"{paramReference}.GetValueOrDefault(""{x.AttributeName}"")", x.DataMember.Name)))
            .ToArray();

        var blockBody = GetAssignments()
            .DefaultAndLast(x => ObjectAssignmentBlock(x.useParentheses, x.assignments, false), x => ObjectAssignmentBlock(x.useParentheses, x.assignments, true))
            .SelectMany(x => x)
            .DefaultIfEmpty("();")
            .Prepend(type.IsTupleType ? "return" : $"return new {FullTypeNameFactory(type)}");

        var method = $"public static {FullTypeNameFactory(type)} {DeserializationMethodNameFactory(type)}(Dictionary<string, AttributeValue> {paramReference})".CreateBlock(blockBody);

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

    private Assignment? StringKeyedPocoGeneric(in KeyValueGeneric keyValueGeneric, in string accessPattern, in string defaultCase, in string memberName)
    {
        switch (keyValueGeneric)
        {
            case {TKey: not {SpecialType: SpecialType.System_String}}:
                return null;
            case {Type: KeyValueGeneric.SupportedType.LookUp}:
                var lookupValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.z", in memberName);
                return new Assignment(
                    $"{accessPattern} switch {{ {{ M: {{ }} x }} => x.SelectMany(y => y.Value.L, (y, z) => (y.Key, z)).ToLookup(y => y.Key, y => {lookupValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    lookupValueAssignment.KnownType
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = DataMemberAssignment(keyValueGeneric.TValue, "y.Value", in memberName);
                return new Assignment(
                    $"{accessPattern} switch {{ {{ M: {{ }} x }} => x.ToDictionary(y => y.Key, y => {dictionaryValueAssignment.Value}), {defaultCase} }}",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.KnownType
                );
            default:
                return null;
        }
    }
    private Assignment? StringKeyedValuedGeneric(in KeyValueGeneric keyValueGeneric, in string accessPattern)
    {
        switch (keyValueGeneric)
        {
            case {TKey: not {SpecialType: SpecialType.System_String}}:
                return null;
            case {Type: KeyValueGeneric.SupportedType.LookUp}:
                var lookupValueAssignment = BuildList(keyValueGeneric.TValue, "x");
                return new Assignment(
                    $"M = {accessPattern}.ToDictionary(x => x.Key, x => {lookupValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    lookupValueAssignment.KnownType
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = AttributeValueAssignment(keyValueGeneric.TValue, "x.Value");
                return new Assignment(
                    $"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionaryValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.KnownType
                );
            default:
                return null;
        }
    }
}