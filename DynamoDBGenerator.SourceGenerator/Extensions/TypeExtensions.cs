using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class TypeExtensions
{
    public static DynamoDBKeyStructure? GetKeyStructure(this IEnumerable<DynamoDbDataMember> dataMembers)
    {
        var items = dataMembers
            .SelectMany(x => x.Attributes, (x, y) => (DataMember: x, Attribute: y))
            .ToArray();

        var partitionKey = items
            .Where(x => x.Attribute is DynamoDBHashKeyAttribute)
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();

        var rangeKey = items
            .Where(x => x.Attribute is DynamoDBRangeKeyAttribute)
            .Select(x => x.DataMember)
            .Cast<DynamoDbDataMember?>()
            .FirstOrDefault();

        if (partitionKey is null)
            return null;

        var lsi = items
            .Where(x => x.Attribute is DynamoDBLocalSecondaryIndexRangeKeyAttribute)
            .SelectMany(x =>
                {
                    var lsi = (DynamoDBLocalSecondaryIndexRangeKeyAttribute)x.Attribute;
                    return lsi.IndexNames;
                },
                (x, y) => new LocalSecondaryIndex(x.DataMember, y)
            )
            .ToArray();

        var gsi = items
            .Where(x => x.Attribute is DynamoDBGlobalSecondaryIndexHashKeyAttribute or DynamoDBGlobalSecondaryIndexRangeKeyAttribute)
            .GroupBy(x => x.Attribute switch
            {
                DynamoDBGlobalSecondaryIndexHashKeyAttribute gsiHash when gsiHash.IndexNames.Length > 0 => gsiHash.IndexNames[0],
                DynamoDBGlobalSecondaryIndexRangeKeyAttribute gsiRange when gsiRange.IndexNames.Length > 0 => gsiRange.IndexNames[0],
                _ => throw new NotSupportedException(x.DataMember.DataMember.Type.ToDisplayString())
            })
            .Select(x =>
            {

                var gsiPartitionKey = x
                    .Where(y => y.Attribute is DynamoDBGlobalSecondaryIndexHashKeyAttribute)
                    .Select(y => y.DataMember)
                    .Cast<DynamoDbDataMember?>()
                    .FirstOrDefault();

                var gsiRangeKey = x
                    .Where(y => y.Attribute is DynamoDBGlobalSecondaryIndexRangeKeyAttribute)
                    .Select(y => y.DataMember)
                    .Cast<DynamoDbDataMember?>()
                    .FirstOrDefault();

                if (gsiPartitionKey is null)
                    throw new NotSupportedException("Could not determine GSI");

                return new GlobalSecondaryIndex(gsiPartitionKey.Value, gsiRangeKey, x.Key);

            })
            .ToArray();
        
        return new DynamoDBKeyStructure(partitionKey.Value, rangeKey, lsi, gsi);
    }

    public static Func<ITypeSymbol, string> NameCache(SymbolDisplayFormat symbolDisplayFormat, IEqualityComparer<ISymbol> comparer)
    {
        var dictionary = new Dictionary<ISymbol, string>(comparer);

        return x =>
        {
            if (dictionary.TryGetValue(x, out var name))
                return name;

            name = x.ToDisplayString(symbolDisplayFormat);

            dictionary.Add(x, name);
            return name;
        };
    }
    public static Func<ITypeSymbol, DynamoDbDataMember[]> DataMembers(IEqualityComparer<ISymbol> comparer)
    {
        var cache = new Dictionary<ITypeSymbol, DynamoDbDataMember[]>(comparer);

        return x =>
        {
            if (cache.TryGetValue(x, out var dynamoDbDataMembers))
                return dynamoDbDataMembers;

            var properties = x.GetDynamoDbProperties().ToArray();
            cache[x] = properties;

            return properties;
        };
    }
    public static Func<ITypeSymbol, string> TypeSymbolStringCache(string suffix, IEqualityComparer<ISymbol> comparer, bool useNullableAnnotationNaming)
    {
        return x => Execution(
            new Dictionary<ITypeSymbol, string>(comparer),
            x,
            false,
            suffix,
            useNullableAnnotationNaming
        );

        static string Execution(
            IDictionary<ITypeSymbol, string> cache,
            ITypeSymbol typeSymbol,
            bool isRecursive,
            string suffix,
            bool useNullableAnnotationNaming
        )
        {
            if (cache.TryGetValue(typeSymbol, out var methodName))
                return methodName;

            var displayString = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var str = useNullableAnnotationNaming
                ? (typeSymbol.NullableAnnotation, typeDisplay: displayString) switch
                {
                    (NullableAnnotation.NotAnnotated, _) => $"NN_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    (NullableAnnotation.None, _) => $"{displayString.ToAlphaNumericMethodName()}{suffix}",
                    (NullableAnnotation.Annotated, _) => $"N_{displayString.ToAlphaNumericMethodName()}{suffix}",
                    _ => throw new NotImplementedException(typeSymbol.ToDisplayString())
                }
                : $"{displayString.ToAlphaNumericMethodName()}{suffix}";

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                // We do not need to populate the dictionary if the execution originates from recursion.
                if (isRecursive is false)
                    cache[typeSymbol] = str;

                return str;
            }

            var result = string.Join(
                "_",
                namedTypeSymbol.TypeArguments.Select(x => Execution(cache, x, true, suffix, useNullableAnnotationNaming)).Prepend(str)
            );

            // We do not need to populate the dictionary if the execution originates from recursion.
            if (isRecursive is false)
                cache[typeSymbol] = result;

            return result;
        }
    }
    public static Assignment ToInlineAssignment(this ITypeSymbol typeSymbol, string value)
    {
        return new Assignment(in value, in typeSymbol, false);
    }

    public static Assignment ToExternalDependencyAssignment(this ITypeSymbol typeSymbol, string value)
    {
        return new Assignment(in value, in typeSymbol, true);
    }

    public static INamedTypeSymbol? TryGetNullableValueType(this ITypeSymbol type)
    {
        return type.IsValueType && type is INamedTypeSymbol {OriginalDefinition.SpecialType: SpecialType.System_Nullable_T} symbol ? symbol : null;
    }


    public static string ToXmlComment(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol {IsGenericType: true} namedTypeSymbol)
            return typeSymbol.ToDisplayString();

        var typeParameters = string.Join(",", namedTypeSymbol.TypeParameters.Select(x => x.Name));

        return Regex.Replace(namedTypeSymbol.ToDisplayString(), "<.+>", $"{{{typeParameters}}}");
    }


    public static KnownType? GetKnownType(this ITypeSymbol type)
    {
        if (BaseType.CreateInstance(type) is { } baseType)
            return baseType;

        if (SingleGeneric.CreateInstance(type) is { } singleGeneric)
            return singleGeneric;

        if (KeyValueGeneric.CreateInstance(type) is { } keyValueGeneric)
            return keyValueGeneric;

        return null;
    }
    public static bool IsNumeric(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType
            is SpecialType.System_Int16
            or SpecialType.System_Byte
            or SpecialType.System_Int32
            or SpecialType.System_Int64
            or SpecialType.System_SByte
            or SpecialType.System_UInt16
            or SpecialType.System_UInt32
            or SpecialType.System_UInt64
            or SpecialType.System_Decimal
            or SpecialType.System_Double
            or SpecialType.System_Single;
    }
}