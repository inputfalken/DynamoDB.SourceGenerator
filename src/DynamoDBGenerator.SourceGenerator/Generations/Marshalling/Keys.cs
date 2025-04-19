using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator;

namespace DynamoDBGenerator.SourceGenerator.Generations.Marshalling;

internal static partial class Marshaller
{
    internal static class KeyMarshaller
    {
        private const string DictionaryName = "attributeValues";
        private const string EnforcePkReference = "isPartitionKey";
        private const string EnforceRkReference = "isRangeKey";

        private static readonly Func<ITypeSymbol, string> MethodName =
            TypeExtensions.SuffixedTypeSymbolNameFactory(null, SymbolEqualityComparer.IncludeNullability);

        private const string PkReference = "partitionKey";
        private const string RkReference = "rangeKey";

        private static IEnumerable<string> CreateAssignment(string validateReference, string keyReference,
            DynamoDbDataMember dataMember, MarshallerOptions options)
        {
            const string reference = "value";
            var expression = $"{keyReference} is {dataMember.DataMember.TypeIdentifier.UnannotatedString} {{ }} {reference}";

            var innerContent = $"if ({expression}) "
                .CreateScope(
                    $@"{DictionaryName}.Add(""{dataMember.AttributeName}"", {InvokeMarshallerMethod(dataMember.DataMember.TypeIdentifier, reference, $"nameof({keyReference})", options)});")
                .Concat($"else if ({keyReference} is null) ".CreateScope(
                    $@"throw {ExceptionHelper.KeysArgumentNullExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"");"))
                .Concat("else".CreateScope(
                    $@"throw {ExceptionHelper.KeysInvalidConversionExceptionMethod}(""{dataMember.DataMember.Name}"", ""{keyReference}"", {keyReference}, ""{dataMember.DataMember.TypeIdentifier.UnannotatedString}"");"));

            return $"if ({validateReference})".CreateScope(innerContent);
        }

        private static IEnumerable<string> MethodBody(ITypeSymbol typeSymbol,
            Func<ITypeSymbol, DynamoDbDataMember[]> fn, MarshallerOptions options)
        {
            var keyStructure = DynamoDbDataMember.GetKeyStructure(fn(typeSymbol));
            if (keyStructure is null)
            {
                yield return @$"throw {ExceptionHelper.NoDynamoDBKeyAttributesExceptionMethod}(""{typeSymbol}"");";

                yield break;
            }

            yield return $"var {DictionaryName} = new Dictionary<string, AttributeValue>(2);";

            var switchBody = GetAssignments(keyStructure.Value, options)
                .SelectMany(x =>
                    $"case {(x.IndexName is null ? "null" : @$"""{x.IndexName}""")}:".CreateScope(x.assignments)
                        .Append("break;"))
                .Append($"default: throw {ExceptionHelper.MissMatchedIndexNameExceptionMethod}(nameof(index), index);");

            foreach (var s in "switch (index)".CreateScope(switchBody))
                yield return s;

            var validateSwitch = $"if ({EnforcePkReference} && {EnforceRkReference} && {DictionaryName}.Count == 2)"
                .CreateScope($"return {DictionaryName};")
                .Concat($"if ({EnforcePkReference} && {EnforceRkReference} is false && {DictionaryName}.Count == 1)"
                    .CreateScope($"return {DictionaryName};"))
                .Concat($"if ({EnforcePkReference} is false && {EnforceRkReference} && {DictionaryName}.Count == 1)"
                    .CreateScope($"return {DictionaryName};"))
                .Concat($"if ({EnforcePkReference} && {EnforceRkReference} && {DictionaryName}.Count == 1)".CreateScope(
                    $"throw {ExceptionHelper.KeysMissingDynamoDBAttributeExceptionMethod}({PkReference}, {RkReference});"))
                .Append($"throw {ExceptionHelper.ShouldNeverHappenExceptionMethod}();");

            foreach (var s in validateSwitch)
                yield return s;
        }

        internal static IEnumerable<string> CreateKeys(IEnumerable<DynamoDBMarshallerArguments> arguments,
            Func<ITypeSymbol, DynamoDbDataMember[]> getDynamoDbProperties, MarshallerOptions options)
        {
            var hashSet = new HashSet<TypeIdentifier>(TypeIdentifier.Nullable);

            return arguments
                .SelectMany(x => CodeFactory.Create(x.EntityTypeSymbol,
                    y => StaticAttributeValueDictionaryKeys(y, getDynamoDbProperties, options), hashSet));
        }

        private static IEnumerable<(string? IndexName, IEnumerable<string> assignments)> GetAssignments(
            DynamoDBKeyStructure keyStructure, MarshallerOptions options)
        {
            yield return keyStructure switch
            {
                { PartitionKey: var pk, SortKey: { } sortKey } => (null,
                    CreateAssignment(EnforcePkReference, PkReference, pk, options)
                        .Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey, options))),
                { PartitionKey: var pk, SortKey: null } => (null,
                    CreateAssignment(EnforcePkReference, PkReference, pk, options)
                        .Concat(MissingAssigment(EnforceRkReference, RkReference)))
            };

            foreach (var gsi in keyStructure.GlobalSecondaryIndices)
            {
                yield return gsi switch
                {
                    { PartitionKey: var pk, SortKey: { } sortKey } => (gsi.Name,
                        CreateAssignment(EnforcePkReference, PkReference, pk, options)
                            .Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey, options))),
                    { PartitionKey: var pk, SortKey: null } => (gsi.Name,
                        CreateAssignment(EnforcePkReference, PkReference, pk, options)
                            .Concat(MissingAssigment(EnforceRkReference, RkReference)))
                };
            }

            foreach (var lsi in keyStructure.LocalSecondaryIndices)
            {
                yield return (lsi, keyStructure.PartitionKey) switch
                {
                    { PartitionKey: var pk, lsi: var sortKey } => (lsi.Name,
                        CreateAssignment(EnforcePkReference, PkReference, pk, options)
                            .Concat(CreateAssignment(EnforceRkReference, RkReference, sortKey.SortKey, options)))
                };
            }
        }

        private static IEnumerable<string> MissingAssigment(string validateReference, string keyReference)
        {
            var expression = $"{validateReference} && {keyReference} is not null";
            return $"if ({expression})".CreateScope(
                $"throw {ExceptionHelper.KeysValueWithNoCorrespondenceMethod}(\"{keyReference}\", {keyReference});");
        }

        internal static IEnumerable<string> IndexKeyMarshallerRootSignature(ITypeSymbol typeSymbol)
        {
            return
                $"public {Constants.DynamoDBGenerator.Marshaller.IndexKeyMarshallerInterface} IndexKeyMarshaller(string index)"
                    .CreateScope(
                        "ArgumentNullException.ThrowIfNull(index);",
                        $"return new {IndexKeyMarshallerImplementationTypeName}((pk, rk, ipk, irk, dm) => {ClassName}.{MethodName(typeSymbol)}({MarshallerOptions.FieldReference}, pk, rk, ipk, irk, dm), index);"
                    );
        }

        public const string PrimaryKeyMarshallerReference = "PrimaryKeyMarshaller";

        public const string PrimaryKeyMarshallerDeclaration =
            $"public {Constants.DynamoDBGenerator.Marshaller.KeyMarshallerInterface} {PrimaryKeyMarshallerReference} {{ get; }}";

        internal static string AssignmentRoot(ITypeSymbol typeSymbol)
        {
            return
                $"new {KeyMarshallerImplementationTypeName}((pk, rk, ipk, irk, dm) => {ClassName}.{MethodName(typeSymbol)}({MarshallerOptions.FieldReference}, pk, rk, ipk, irk, dm))";
        }

        private static CodeFactory StaticAttributeValueDictionaryKeys(TypeIdentifier typeIdentifier,
            Func<ITypeSymbol, DynamoDbDataMember[]> fn, MarshallerOptions options)
        {
            var code =
                $"public static Dictionary<string, AttributeValue> {MethodName(typeIdentifier.TypeSymbol)}({options.FullName} {MarshallerOptions.ParamReference}, object? {PkReference}, object? {RkReference}, bool {EnforcePkReference}, bool {EnforceRkReference}, string? index = null)"
                    .CreateScope(MethodBody(typeIdentifier.TypeSymbol, fn, options));

            return new CodeFactory(code);
        }
    }
}