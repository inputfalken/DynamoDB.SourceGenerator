using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public class Generation
{
    private readonly ITypeSymbol _rootTypeSymbol;
    private const string Indent = "                ";
    private readonly string _rootExpressionAttributeNames;

    public Generation(in ITypeSymbol typeSymbol)
    {
        _rootTypeSymbol = typeSymbol;
        _rootExpressionAttributeNames = CreateExpressionAttributeNamesClass(typeSymbol);
    }

    /// <summary>
    /// Creates an Dictionary with string as key and AttributeValue as value.
    /// </summary>
    public SourceGeneratedCode CreateAttributeValueFactory(
        in ConsumerMethodConfiguration methodConfiguration,
        KeyStrategy keyStrategy)
    {
        var className = $"{_rootTypeSymbol.Name}_{methodConfiguration.Name}";
        var consumerMethod = RootMethod(
            "Dictionary<string, AttributeValue>",
            className,
            CreateMethodName(_rootTypeSymbol),
            methodConfiguration
        );

        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                x => StaticAttributeValueDictionaryFactory(x, keyStrategy),
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability)
            )
            .Select(static x => x.Code);

        var sourceGeneratedCode = string.Join(Constants.NewLine, enumerable);

        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Private,
            in className,
            in sourceGeneratedCode,
            indentLevel: 2
        );

        var code = $@"{consumerMethod}
{@class}";
        return new SourceGeneratedCode(code, className, methodConfiguration.Name);
    }

    // TODO come up with a solution to make the following intuitive 
    // `ConditionExpression = "#S <> :status"`
    // `UpdateExpression = "SET #S = :status, #MD.#DT = :statusUpdatedAt"`
    // Perhaps having a class constants for the respective use case.
    // Like : ExpressionAttribute.Values.{Type}.Status & ExpressionAttribute.Names.{Type}.Status
    // ExpressionAttribute.Values.UpdateStatus.Status
    // ExpressionAttribute.Names.UpdateStatus.Status
    // ExpressionAttribute.Values.UpdateStatus.Metadata.ModifiedAt
    // ExpressionAttribute.Names.UpdateStatus.Metadata.ModifiedAt

    // Or perhaps  pass a custom generated class for the ExpressionAttribute
    // UpdateStatusExpressionAttribute expressionAttributes = GetExpressionAttributes();  
    // expressionAttributes.Value.Status;
    // expressionAttributes.Name.Status;
    // expressionAttributes.Value.Status.Metadata.ModifiedAt;
    // expressionAttributes.Name.Status.Metadata.ModifiedAt;

    // TODO create a method that can generate the following 
    //        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    //        {
    //            {":status", new AttributeValue {S = determinedReplacement.Status}},
    //            {":statusUpdatedAt", new AttributeValue {S = DateTimeOffset.UtcNow.ToIso8601()}}
    //        },
    public string CreateExpressionAttributeValues()
    {
        throw new NotImplementedException();
    }


    // TODO create a method that can generate the following 
    //        ExpressionAttributeNames =
    //            new Dictionary<string, string>
    //            {
    //                {"#S", nameof(ReplacementEntity.Status)},
    //                {"#DT", nameof(ReplacementEntity.Metadata.StatusModifiedAt)},
    //                {"#MD", nameof(ReplacementEntity.Metadata)},
    //            },
    public string CreateExpressionAttributeNames()
    {
        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                StaticExpressionAttributeNamesFactory,
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability)
            )
            .Select(static x => x.Code);

        var sourceGeneratedCode = string.Join(Constants.NewLine, enumerable);

        var className = $"{_rootTypeSymbol.Name}_ExpressionAttribute";
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            className,
            in sourceGeneratedCode,
            indentLevel: 2
        );

        return
            $@"public {className}.{_rootExpressionAttributeNames} Get{_rootTypeSymbol.Name}AttributeNames() => new {className}.{_rootExpressionAttributeNames}();
{@class}
";
    }

    private Conversion StaticExpressionAttributeNamesFactory(ITypeSymbol typeSymbol)
    {
        var isRoot = _rootTypeSymbol.Equals(typeSymbol, SymbolEqualityComparer.Default);
        var dataMembers = typeSymbol.GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(x => (IsKnown: x.DataMember.Type.GetKnownType() is not null, DDB: x))
            .ToArray();

        var fieldDeclarations = dataMembers.Select(x =>
            x.IsKnown
                ? $@"public string {x.DDB.DataMember.Name} {{ get; }}"
                : $@"public {x.DDB.DataMember.Type.Name}ExpressionAttributeNames {x.DDB.DataMember.Name} {{ get; }}");

        var dictionaryFields = dataMembers
            .Where(x => x.IsKnown)
            .Select(x => x.DDB)
            .Select(x => isRoot
                ? $@"{{""#{x.AttributeName}"", ""{x.AttributeName}""}}"
                : $@"{{$""{{prefix}}.{x.AttributeName}"", ""{x.AttributeName}""}}"
            );

        var dictionary = $"new Dictionary<string, string> {{{string.Join(", ", dictionaryFields)}}}";
        var className = CreateExpressionAttributeNamesClass(typeSymbol);

        var fieldAssignments = dataMembers
            .Select(x =>
            {
                var assignment = (AssignedBy: x.IsKnown, isRoot) switch
                {
                    (true, true) => $@"{x.DDB.DataMember.Name} = ""#{x.DDB.AttributeName}"";",
                    (true, false) => $@"{x.DDB.DataMember.Name} = $""{{prefix}}.#{x.DDB.AttributeName}"";",
                    (false, true) =>
                        $@"{x.DDB.DataMember.Name} = new {x.DDB.DataMember.Type.Name}ExpressionAttributeNames(""#{x.DDB.AttributeName}"");",
                    (false, false) =>
                        $@"{x.DDB.DataMember.Name} = new {x.DDB.DataMember.Type.Name}ExpressionAttributeNames($""{{prefix}}.#{x.DDB.AttributeName}"");",
                };

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var indent = StringExtensions.Indent(2);
        var constructor = $@"public {className}({(isRoot ? null : "string prefix")}) : base ({dictionary}) 
    {{
        {string.Join($"{Constants.NewLine}{indent}", fieldAssignments.Select(x => x.Value))}
    }}";
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className} : Dictionary<string, string>",
            $@"{constructor}
{indent}{string.Join($"{Constants.NewLine}{indent}", fieldDeclarations)}",
            0
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));
    }

    private static string CreateExpressionAttributeNamesClass(ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.Name}ExpressionAttributeNames";
    }


    private string RootMethod(string returnType, string className, string methodName,
        ConsumerMethodConfiguration config)
    {
        var accessModifier = config.AccessModifier.ToCode();
        var signature = config.MethodParameterization switch
        {
            ConsumerMethodConfiguration.Parameterization.UnparameterizedInstance =>
                $"{accessModifier} {returnType} {config.Name}() => {className}.{methodName}(this);",
            ConsumerMethodConfiguration.Parameterization.ParameterizedStatic =>
                $"{accessModifier} static {returnType} {config.Name}({_rootTypeSymbol.ToDisplayString()} item) => {className}.{methodName}(item);",
            ConsumerMethodConfiguration.Parameterization.ParameterizedInstance =>
                $"{accessModifier} {returnType} {config.Name}({_rootTypeSymbol.ToDisplayString()} item) => {className}.{methodName}(item);",
            _ => throw new NotSupportedException($"Config of '{config.MethodParameterization}'.")
        };


        return $@"/// <summary> 
        ///    Converts <see cref=""{_rootTypeSymbol.ToXmlComment()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        {signature}";
    }

    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, KeyStrategy keyStrategy)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";

        var dynamoDbProperties = keyStrategy switch
        {
            KeyStrategy.Ignore => type.GetDynamoDbProperties()
                .Where(static x => x is {IsRangeKey: false, IsHashKey: false}),
            KeyStrategy.Only => type.GetDynamoDbProperties()
                .Where(static x => x is {IsRangeKey: true, IsHashKey: true}),
            KeyStrategy.Include => type.GetDynamoDbProperties(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var properties = dynamoDbProperties
            .Where(static x => x.IsIgnored is false)
            .Select(static x => (
                    AccessPattern: $"{paramReference}.{x.DataMember.Name}",
                    DDB: x
                )
            )
            .Select(x => (
                x.AccessPattern,
                x.DDB,
                AttributeValue: AttributeValueAssignment(
                    x.DDB.DataMember.Type,
                    x.AccessPattern
                )
            ))
            .Where(x => keyStrategy switch
            {
                // We skip creating assignments of external dependencies if the intent is only to fetch keys.
                KeyStrategy.Only => x.AttributeValue.HasExternalDependency is false,
                _ => true
            })
            .Select(static x => (
                    x.DDB,
                    x.AttributeValue,
                    DictionaryAssignment: x.DDB.DataMember.Type.NotNullIfStatement(
                        in x.AccessPattern,
                        @$"{dictionaryName}.Add(""{x.DDB.AttributeName}"", {x.AttributeValue.ToAttributeValue()});"
                    ),
                    CapacityTernaries: x.DDB.DataMember.Type.NotNullTernaryExpression(in x.AccessPattern, "1", "0")
                )
            )
            .ToArray();

        var dictionary =
            @$"        
            /// <summary> 
            ///    Converts <see cref=""{type.ToXmlComment()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
            /// </summary>
            /// <remarks> 
            ///    This method should only be invoked by source generated code.
            /// </remarks>
            public static Dictionary<string, AttributeValue> {CreateMethodName(type)}({type.ToDisplayString(topLevelNullability: NullableFlowState.None)} {paramReference})
            {{ 
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.CapacityTernaries))}
                {string.Join(Constants.NewLine + Indent, properties.Select(static x => x.DictionaryAssignment))}
                return {dictionaryName};
            }}";


        var conversions = keyStrategy switch
        {
            // We skip considering creating external methods if the intent is to fetch keys only.
            KeyStrategy.Only => Enumerable.Empty<Assignment>(),
            _ => properties
                .Select(static x => x.AttributeValue)
                .Where(static x => x.HasExternalDependency)
        };
        return new Conversion(in dictionary, in conversions);

        static string InitializeDictionary(string dictionaryName, IEnumerable<string> capacityCalculations)
        {
            var capacityCalculation = string.Join(" + ", capacityCalculations);

            return string.Join(" + ", capacityCalculation)switch
            {
                "" => $"var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: 0);",
                var capacities => $@"var capacity = {capacities};
                var {dictionaryName} = new Dictionary<string, AttributeValue>(capacity: capacity);
                if (capacity is 0) {{ return {dictionaryName}; }} "
            };
        }
    }

    private Assignment BuildList(in ITypeSymbol elementType, in string accessPattern)
    {
        var attributeValue = AttributeValueAssignment(elementType, "x");
        var select = $"Select(x => {attributeValue.ToAttributeValue()}))";
        var assignment = elementType.NotNullLambdaExpression() is { } whereBody
            ? $"L = new List<AttributeValue>({accessPattern}.Where({whereBody}).{select}"
            : $"L = new List<AttributeValue>({accessPattern}.{select}";

        return new Assignment(in assignment, in elementType, attributeValue.HasExternalDependency);
    }

    private static Assignment? BuildSet(in ITypeSymbol elementType, in string accessPattern)
    {
        var newAccessPattern = elementType.NotNullLambdaExpression() is { } expression
            ? $"{accessPattern}.Where({expression})"
            : accessPattern;

        if (elementType.SpecialType is SpecialType.System_String)
            return new Assignment(
                $"SS = new List<string>({newAccessPattern})",
                in elementType,
                false
            );

        return elementType.IsNumeric() is false
            ? null
            : new Assignment(
                $"NS = new List<string>({newAccessPattern}.Select(x => x.ToString()))",
                in elementType,
                false
            );
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
                    lookupValueAssignment.HasExternalDependency
                );
            case {Type: KeyValueGeneric.SupportedType.Grouping}:
                var groupingValueAssignment = BuildList(keyValueGeneric.TValue, accessPattern);
                return new Assignment(
                    $@"M = new Dictionary<string, AttributeValue>{{ {{ {accessPattern}.Key, {groupingValueAssignment.ToAttributeValue()}}} }}",
                    keyValueGeneric.TValue,
                    groupingValueAssignment.HasExternalDependency
                );
            case {Type: KeyValueGeneric.SupportedType.Dictionary}:
                var dictionaryValueAssignment = AttributeValueAssignment(keyValueGeneric.TValue, "x.Value");
                return new Assignment(
                    $@"M = {accessPattern}.ToDictionary(x => x.Key, x => {dictionaryValueAssignment.ToAttributeValue()})",
                    keyValueGeneric.TValue,
                    dictionaryValueAssignment.HasExternalDependency
                );
            default:
                return null;
        }
    }

    private Assignment AttributeValueAssignment(in ITypeSymbol typeSymbol, in string accessPattern)
    {
        if (typeSymbol.GetKnownType() is not { } knownType) return ExternalAssignment(in typeSymbol, in accessPattern);

        var assignment = knownType switch
        {
            BaseType baseType => baseType.Type switch
            {
                BaseType.SupportedType.String => typeSymbol.ToInlineAssignment($"S = {accessPattern}"),
                BaseType.SupportedType.Bool => typeSymbol.ToInlineAssignment($"BOOL = {accessPattern}"),
                BaseType.SupportedType.Number => typeSymbol.ToInlineAssignment($"N = {accessPattern}.ToString()"),
                BaseType.SupportedType.Char => typeSymbol.ToInlineAssignment($"S = {accessPattern}.ToString()"),
                BaseType.SupportedType.Temporal => typeSymbol.ToInlineAssignment(
                    $@"S = {accessPattern}.ToString(""O"")"),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            SingleGeneric singleGeneric => singleGeneric.Type switch
            {
                SingleGeneric.SupportedType.Collection => BuildList(singleGeneric.T, in accessPattern),
                SingleGeneric.SupportedType.Nullable => AttributeValueAssignment(singleGeneric.T,
                    $"{accessPattern}.Value"),
                SingleGeneric.SupportedType.Set => BuildSet(singleGeneric.T, accessPattern),
                _ => throw new ArgumentOutOfRangeException(typeSymbol.ToDisplayString())
            },
            KeyValueGeneric keyValueGeneric => StringKeyedValuedGeneric(in keyValueGeneric, in accessPattern),
            _ => null
        };

        return assignment ?? ExternalAssignment(in typeSymbol, in accessPattern);

        Assignment ExternalAssignment(in ITypeSymbol typeSymbol, in string accessPattern) =>
            typeSymbol.ToExternalDependencyAssignment($"M = {CreateMethodName(typeSymbol)}({accessPattern})");
    }

    private readonly IDictionary<ITypeSymbol, string> _methodNameCache =
        new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.IncludeNullability);

    private string CreateMethodName(ITypeSymbol typeSymbol)
    {
        return Execution(_methodNameCache, typeSymbol, false);

        static string Execution(
            IDictionary<ITypeSymbol, string> cache,
            ITypeSymbol typeSymbol,
            bool isRecursive
        )
        {
            if (cache.TryGetValue(typeSymbol, out var methodName))
                return methodName;

            var displayString = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var str = (typeSymbol.NullableAnnotation, typeDisplay: displayString) switch
            {
                (_, {Length: > Constants.MaxMethodNameLenght}) => throw new NotSupportedException(
                    $"Could not generate a method name that's within the supported method lenght {Constants.MaxMethodNameLenght} for type '{displayString}'."),
                (NullableAnnotation.NotAnnotated, _) => $"NN_{displayString.ToAlphaNumericMethodName()}Factory",
                (NullableAnnotation.None, _) => $"{displayString.ToAlphaNumericMethodName()}Factory",
                (NullableAnnotation.Annotated, _) => $"N_{displayString.ToAlphaNumericMethodName()}Factory",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                // We do not need to populate the dictionary if the execution originates from recursion.
                if (isRecursive is false)
                    cache.Add(typeSymbol, str);

                return str;
            }

            var result = string.Join(
                "_",
                namedTypeSymbol.TypeArguments.Select(x => Execution(cache, x, true)).Prepend(str)
            );

            // We do not need to populate the dictionary if the execution originates from recursion.
            if (isRecursive is false)
                cache.Add(typeSymbol, result);

            return result;
        }
    }
}