using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;
namespace DynamoDBGenerator.SourceGenerator.Generations;

public static class KeyMarshaller
{
    private const string DictionaryName = "attributeValues";
    private const string EnforcePkReference = "isPartitionKey";
    private const string EnforceRkReference = "isRangeKey";
    private static readonly Func<ITypeSymbol, string> MethodName = TypeExtensions.SuffixedTypeSymbolNameFactory("Keys", SymbolEqualityComparer.IncludeNullability);
    private const string PkReference = "partitionKey";
    private const string RkReference = "rangeKey";
    private static IEnumerable<string> CreateAssignment(string validateReference, string keyReference, DynamoDbDataMember dataMember)
    {
        const string reference = "value";
        var expectedType = dataMember.DataMember.Type.Representation().original;
        var expression = $"{keyReference} is {expectedType} {{ }} {reference}";

        var innerContent = $"if ({expression}) "
            .CreateBlock($@"{DictionaryName}.Add(""{dataMember.AttributeName}"", {Marshaller.InvokeMarshallerMethod(dataMember.DataMember.Type, reference, $"nameof({keyReference})")});")
            .Concat($"else if ({keyReference} is null) ".CreateBlock($@"throw {ExceptionHelper.KeysArgumentNullExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"");"))
            .Concat("else".CreateBlock($@"throw {ExceptionHelper.KeysInvalidConversionExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"", {keyReference}, ""{expectedType}"");"));

        return $"if({validateReference})".CreateBlock(innerContent);

    }
    private static IEnumerable<string> CreateBody(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {
        var keyStructure = DynamoDbDataMember.GetKeyStructure(fn(typeSymbol));
        if (keyStructure is null)
        {
            yield return @$"throw {ExceptionHelper.NoDynamoDBKeyAttributesExceptionMethod}(""{typeSymbol}"");";

            yield break;
        }

        yield return $"var {DictionaryName} = new Dictionary<string, AttributeValue>(2);";

        var switchBody = GetAssignments(keyStructure.Value)
            .SelectMany(x => $"case {(x.IndexName is null ? "null" : @$"""{x.IndexName}""")}:".CreateBlock(x.assignments).Append("break;"))
            .Append($"default: throw {ExceptionHelper.MissMatchedIndexNameExceptionMethod}(nameof(index), index);");

        foreach (var s in "switch (index)".CreateBlock(switchBody))
            yield return s;

        var validateSwitch = $"if ({EnforcePkReference} && {EnforceRkReference} && {DictionaryName}.Count == 2)"
            .CreateBlock($"return {DictionaryName};")
            .Concat($"if ({EnforcePkReference} && {EnforceRkReference} is false && {DictionaryName}.Count == 1)".CreateBlock($"return {DictionaryName};"))
            .Concat($"if ({EnforcePkReference} is false && {EnforceRkReference} && {DictionaryName}.Count == 1)".CreateBlock($"return {DictionaryName};"))
            .Concat($"if ({EnforcePkReference} && {EnforceRkReference} && {DictionaryName}.Count == 1)".CreateBlock($"throw {ExceptionHelper.KeysMissingDynamoDBAttributeExceptionMethod}({PkReference}, {RkReference});"))
            .Append($"throw {ExceptionHelper.ShouldNeverHappenExceptionMethod}();");

        foreach (var s in validateSwitch)
            yield return s;

    }
    internal static IEnumerable<string> CreateKeys(IEnumerable<DynamoDBMarshallerArguments> arguments, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> getDynamoDbProperties)
    {
        var hashSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

        return arguments
            .SelectMany(x => Conversion.ConversionMethods(x.EntityTypeSymbol, y => StaticAttributeValueDictionaryKeys(y, getDynamoDbProperties), hashSet)).SelectMany(x => x.Code);
    }
    private static IEnumerable<(string? IndexName, IEnumerable<string> assignments)> GetAssignments(DynamoDBKeyStructure keyStructure)
    {
        yield return keyStructure switch
        {
            {PartitionKey: var pk, SortKey: { } sortKey} => (null, CreateAssignment(EnforcePkReference, PkReference, pk).Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey))),
            {PartitionKey: var pk, SortKey: null} => (null, CreateAssignment(EnforcePkReference, PkReference, pk).Concat(MissingAssigment(EnforceRkReference, RkReference)))
        };

        foreach (var gsi in keyStructure.GlobalSecondaryIndices)
        {
            yield return gsi switch
            {
                {PartitionKey: var pk, SortKey: { } sortKey} => (gsi.Name, CreateAssignment(EnforcePkReference, PkReference, pk).Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey))),
                {PartitionKey: var pk, SortKey: null} => (gsi.Name, CreateAssignment(EnforcePkReference, PkReference, pk).Concat(MissingAssigment(EnforceRkReference, RkReference)))
            };
        }

        foreach (var lsi in keyStructure.LocalSecondaryIndices)
        {
            yield return (lsi, keyStructure.PartitionKey) switch
            {
                {PartitionKey: var pk, lsi: var sortKey} => (lsi.Name, CreateAssignment(EnforcePkReference, PkReference, pk).Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey.SortKey)))
            };
        }

    }
    private static IEnumerable<string> MissingAssigment(string validateReference, string keyReference)
    {

        var expression = $"{validateReference} && {keyReference} is not null";
        return $"if ({expression})".CreateBlock($"throw {ExceptionHelper.KeysValueWithNoCorrespondenceMethod}(\"{keyReference}\", {keyReference});");
    }
    internal static IEnumerable<string> IndexKeyMarshaller(ITypeSymbol typeSymbol)
    {
        return $"public {Constants.DynamoDBGenerator.Marshaller.IndexKeyMarshallerInterface} IndexKeyMarshaller(string index)".CreateBlock(
            "ArgumentNullException.ThrowIfNull(index);",
            $"return new {IndexKeyMarshallerImplementationTypeName}({MethodName(typeSymbol)}, index);"
        );
    }
    internal static string PrimaryKeyMarshaller(ITypeSymbol typeSymbol)
    {
        return $"public {Constants.DynamoDBGenerator.Marshaller.KeyMarshallerInterface} PrimaryKeyMarshaller {{ get; }} = new {Constants.DynamoDBGenerator.KeyMarshallerImplementationTypeName}({MethodName(typeSymbol)});";
    }
    private static Conversion StaticAttributeValueDictionaryKeys(ITypeSymbol typeSymbol, Func<ITypeSymbol, IReadOnlyList<DynamoDbDataMember>> fn)
    {

        var code = $"private static Dictionary<string, AttributeValue> {MethodName(typeSymbol)}(object? {PkReference}, object? {RkReference}, bool {EnforcePkReference}, bool {EnforceRkReference}, string? index = null)"
            .CreateBlock(CreateBody(typeSymbol, fn));

        return new Conversion(code);

    }
}