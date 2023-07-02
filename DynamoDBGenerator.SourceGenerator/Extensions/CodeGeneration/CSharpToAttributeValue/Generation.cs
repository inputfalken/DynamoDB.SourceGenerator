using System.ComponentModel;
using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions.CodeGeneration.CSharpToAttributeValue;

public class Generation
{
    private readonly ITypeSymbol _rootTypeSymbol;

    public Generation(in ITypeSymbol typeSymbol)
    {
        _rootTypeSymbol = typeSymbol;
    }

    /// <summary>
    /// Creates an Dictionary with string as key and AttributeValue as value.
    /// </summary>
    public SourceGeneratedCode CreateAttributeValueFactory(
        in MethodConfiguration methodConfiguration,
        KeyStrategy keyStrategy)
    {
        var consumerMethod = CreateMethod(
            _rootTypeSymbol,
            "Dictionary<string, AttributeValue>",
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

        var code = $@"{consumerMethod}
{sourceGeneratedCode}";
        return new SourceGeneratedCode(code,  methodConfiguration.Name);
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
        var i = 0;
        var enumerable = Conversion.ConversionMethods(
                _rootTypeSymbol,
                x => ExpressionAttributeReferencesClassGenerator(x, i++),
                new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default)
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

        return $@"public {nameof(IDynamoDbDocument<object,object>)}<{_rootTypeSymbol.Name}, {className}.{CreateExpressionAttributeNamesClass(_rootTypeSymbol)}> {_rootTypeSymbol.Name}Document {{ get; }} = new {className}.{CreateExpressionAttributeNamesClass(_rootTypeSymbol)}(null);
{@class}";
    }

    private Conversion ExpressionAttributeReferencesClassGenerator(ITypeSymbol typeSymbol, int iterationCount)
    {
        const string valueEnumerableMethodName =
            nameof(IExpressionAttributeReferences<object>.AccessedValues);
        const string nameEnumerableMethodName = nameof(IExpressionAttributeReferences<object>.AccessedNames);
        var interfaceName = AttributeInterfaceName(typeSymbol);

        string? initialImplementation = null;
        if (iterationCount is 0 && typeSymbol.Equals(_rootTypeSymbol, SymbolEqualityComparer.Default))
        {

            var marshalMethods = CreateAttributeValueFactory(new MethodConfiguration($"{typeSymbol.Name}Values")
            {
                AccessModifier = Accessibility.Private,
                MethodParameterization = MethodConfiguration.Parameterization.ParameterizedInstance
            }, KeyStrategy.Include);
            

            var keysMethod = CreateAttributeValueFactory(new MethodConfiguration($"{typeSymbol.Name}Keys")
            {
                AccessModifier = Accessibility.Public,
                MethodParameterization = MethodConfiguration.Parameterization.ParameterizedStatic
            }, KeyStrategy.Only);
            
            var keysClass = CodeGenerationExtensions.CreateClass(Accessibility.Private, "KeysClass", keysMethod.Code, 2);

            initialImplementation = $@"
{marshalMethods.Code}
{keysClass}
{nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {nameof(IDynamoDbDocument<int, int>)}<{typeSymbol.Name},{CreateExpressionAttributeNamesClass(typeSymbol)}>.{nameof(IDynamoDbDocument<object, object>.Marshal)}({_rootTypeSymbol.Name} entity) => {marshalMethods.MethodName}(entity);
{nameof(Dictionary<int, int>)}<{nameof(String)}, {nameof(AttributeValue)}> {nameof(IDynamoDbDocument<int, int>)}<{typeSymbol.Name},{CreateExpressionAttributeNamesClass(typeSymbol)}>.{nameof(IDynamoDbDocument<object, object>.Keys)}({_rootTypeSymbol.Name} entity) => KeysClass.{keysMethod.MethodName}(entity);
{nameof(AttributeExpression<object>)}<{_rootTypeSymbol.Name}> {nameof(IDynamoDbDocument<int, int>)}<{typeSymbol.Name},{CreateExpressionAttributeNamesClass(typeSymbol)}>.{nameof(IDynamoDbDocument<object, object>.UpdateExpression)}(Func<{CreateExpressionAttributeNamesClass(typeSymbol)}, string> selector)
{{
        return new {nameof(AttributeExpression<object>)}<{_rootTypeSymbol.Name}>(this, selector(this));
}}
{nameof(AttributeExpression<object>)}<{_rootTypeSymbol.Name}> {nameof(IDynamoDbDocument<int, int>)}<{typeSymbol.Name},{CreateExpressionAttributeNamesClass(typeSymbol)}>.{nameof(IDynamoDbDocument<object, object>.ConditionExpression)}(Func<{CreateExpressionAttributeNamesClass(typeSymbol)}, string> selector)
{{
        return new {nameof(AttributeExpression<object>)}<{_rootTypeSymbol.Name}>(this, selector(this));
}}
";
        }

        var dataMembers = typeSymbol
            .GetDynamoDbProperties()
            .Where(static x => x.IsIgnored is false)
            .Select(static x => (IsKnown: x.DataMember.Type.GetKnownType() is not null, DDB: x, NameRef: $"_{x.DataMember.Name}NameRef", ValueRef: $"_{x.DataMember.Name}ValueRef"))
            .ToArray();

        var fieldDeclarations = dataMembers
            .SelectMany(static x =>
            {
                if (x.IsKnown)
                    return new[]
                    {
                        $@"    private readonly Lazy<string> {x.ValueRef};",
                        $@"    private readonly Lazy<string> {x.NameRef};",
                        $@"    public AttributeReference {x.DDB.DataMember.Name} {{ get; }}"
                    };

                var typeName = CreateExpressionAttributeNamesClass(x.DDB.DataMember.Type);
                return new[]
                {
                    $@"    private readonly Lazy<{typeName}> _{x.DDB.DataMember.Name};",
                    $@"    public {typeName} {x.DDB.DataMember.Name} => _{x.DDB.DataMember.Name}.Value;"
                };
            });

        const string constructorAttributeName = "nameRef";
        const string constructorAttributeValue = "valueRef";
        var fieldAssignments = dataMembers
            .Select(static x =>
            {
                string assignment;
                var ternaryExpressionName =
                    $"{constructorAttributeName} is null ? {@$"""#{x.DDB.AttributeName}"""}: {@$"$""{{{constructorAttributeName}}}.#{x.DDB.AttributeName}"""}";
                if (x.IsKnown)
                {
                    assignment =
                        $@"        {x.ValueRef} = new Lazy<string>(() => "":{x.DDB.AttributeName}"");
        {x.NameRef} = new Lazy<string>(() => {ternaryExpressionName});
        {x.DDB.DataMember.Name} = new AttributeReference(in {x.NameRef}, in {x.ValueRef});";
                }
                else
                {
                    var name = CreateExpressionAttributeNamesClass(x.DDB.DataMember.Type);
                    assignment =
                        $@"        _{x.DDB.DataMember.Name} = new Lazy<{name}>(() => new {name}({ternaryExpressionName}));";
                }

                return new Assignment(assignment, x.DDB.DataMember.Type, x.IsKnown is false);
            })
            .ToArray();

        var className = CreateExpressionAttributeNamesClass(typeSymbol);
        var constructor = $@"public {className}(string? {constructorAttributeName})
    {{
{string.Join(Constants.NewLine, fieldAssignments.Select(x => x.Value))}
    }}";

        var expressionAttributeNameYields = dataMembers.Select(static x => x.IsKnown
            ? $@"       if ({x.NameRef}.IsValueCreated) yield return new KeyValuePair<string, string>({x.NameRef}.Value, ""{x.DDB.AttributeName}"");"
            : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) foreach (var x in ({x.DDB.DataMember.Name} as {AttributeInterfaceName(x.DDB.DataMember.Type)}).{nameEnumerableMethodName}()) {{ yield return x; }}");
        var expressionAttributeValueYields = dataMembers
            .Select(x =>
            {
                var accessPattern = $"entity.{x.DDB.DataMember.Name}";
                return x.IsKnown
                    ? $@"       if ({x.ValueRef}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"yield return new KeyValuePair<string, AttributeValue>({x.ValueRef}.Value, {AttributeValueAssignment(x.DDB.DataMember.Type, $"entity.{x.DDB.DataMember.Name}").ToAttributeValue()});")}"
                    : $@"       if (_{x.DDB.DataMember.Name}.IsValueCreated) {x.DDB.DataMember.Type.NotNullIfStatement(accessPattern, $"foreach (var x in ({x.DDB.DataMember.Name} as {AttributeInterfaceName(x.DDB.DataMember.Type)}).{valueEnumerableMethodName}({accessPattern})) {{ yield return x; }}")}";
            });

        var implementation = iterationCount is 0 ? $"{nameof(IDynamoDbDocument<object, object>)}<{typeSymbol.Name}, {CreateExpressionAttributeNamesClass(typeSymbol)}>, {interfaceName}"
            : interfaceName;
        var @class = CodeGenerationExtensions.CreateClass(
            Accessibility.Public,
            $"{className} : {implementation}",
            $@"{constructor}
{initialImplementation}
{string.Join(Constants.NewLine, fieldDeclarations)}
    IEnumerable<KeyValuePair<string, string>> {interfaceName}.{nameEnumerableMethodName}()
    {{
{string.Join(Constants.NewLine, expressionAttributeNameYields)}
    }}
    IEnumerable<KeyValuePair<string, AttributeValue>> {interfaceName}.{valueEnumerableMethodName}({typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} entity)
    {{
{string.Join(Constants.NewLine, expressionAttributeValueYields)}
    }}",
            0
        );
        return new Conversion(@class, fieldAssignments.Where(x => x.HasExternalDependency));
    }

    private static string AttributeInterfaceName(ITypeSymbol typeSymbol)
    {
        
        return
            $"{nameof(IExpressionAttributeReferences<object>)}<{typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}>";
    }


    private static string CreateMethod(
        ITypeSymbol typeSymbol,
        string returnType,
        string methodName,
        MethodConfiguration config)
    {
        var accessModifier = config.AccessModifier.ToCode();
        var signature = config.MethodParameterization switch
        {
            MethodConfiguration.Parameterization.UnparameterizedInstance =>
                $"{accessModifier} {returnType} {config.Name}() => {methodName}(this);",
            MethodConfiguration.Parameterization.ParameterizedStatic =>
                $"{accessModifier} static {returnType} {config.Name}({typeSymbol.ToDisplayString()} item) => {methodName}(item);",
            MethodConfiguration.Parameterization.ParameterizedInstance =>
                $"{accessModifier} {returnType} {config.Name}({typeSymbol.ToDisplayString()} item) => {methodName}(item);",
            _ => throw new NotSupportedException($"Config of '{config.MethodParameterization}'.")
        };

        return $@"/// <summary> 
        ///    Converts <see cref=""{typeSymbol.ToXmlComment()}""/> into a <see cref=""Amazon.DynamoDBv2.Model.AttributeValue""/> representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        {signature}";
    }

    private Conversion StaticAttributeValueDictionaryFactory(ITypeSymbol type, KeyStrategy keyStrategy)
    {
        const string paramReference = "entity";
        const string dictionaryName = "attributeValues";
        var properties = GetAssignments(type, keyStrategy).ToArray();

        const string indent = "                ";
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
                {InitializeDictionary(dictionaryName, properties.Select(static x => x.capacityTernary))}
                {string.Join(Constants.NewLine + indent, properties.Select(static x => x.dictionaryPopulation))}
                return {dictionaryName};
            }}";

        return new Conversion(
            in dictionary,
            properties
                .Select(static x => x.assignment)
                .Where(static x => x.HasExternalDependency)
        );

        IEnumerable<(string dictionaryPopulation, string capacityTernary, Assignment assignment)> GetAssignments(
            INamespaceOrTypeSymbol typeSymbol,
            KeyStrategy strategy
        )
        {
            var dynamoDbProperties = strategy switch
            {
                KeyStrategy.Ignore => typeSymbol.GetDynamoDbProperties()
                    .Where(static x => x is {IsRangeKey: false, IsHashKey: false}),
                KeyStrategy.Only => typeSymbol.GetDynamoDbProperties()
                    .Where(static x => x.IsHashKey || x.IsRangeKey),
                KeyStrategy.Include => typeSymbol.GetDynamoDbProperties(),
                _ => throw new ArgumentOutOfRangeException()
            };

            foreach (var x in dynamoDbProperties)
            {
                if (x.IsIgnored)
                    continue;

                var accessPattern = $"{paramReference}.{x.DataMember.Name}";
                var attributeValue = AttributeValueAssignment(x.DataMember.Type, accessPattern);
                if (strategy == KeyStrategy.Only && attributeValue.HasExternalDependency)
                    continue;

                var dictionaryAssignment = x.DataMember.Type.NotNullIfStatement(
                    in accessPattern,
                    @$"{dictionaryName}.Add(""{x.AttributeName}"", {attributeValue.ToAttributeValue()});"
                );

                var capacityTernaries = x.DataMember.Type.NotNullTernaryExpression(in accessPattern, "1", "0");

                yield return (dictionaryAssignment, capacityTernaries, attributeValue);
            }
        }

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

    private static string CreateExpressionAttributeNamesClass(ITypeSymbol typeSymbol)
    {
        return $"{typeSymbol.Name}AttributeReferences";
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