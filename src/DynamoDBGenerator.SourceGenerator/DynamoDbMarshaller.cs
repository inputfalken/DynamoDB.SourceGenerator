using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Generations;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator;

public static class DynamoDbMarshaller
{
    private static IEnumerable<string> CreateImplementations(IEnumerable<DynamoDBMarshallerArguments> arguments,
        MarshallerOptions options)
    {
        foreach (var s in options.ClassDeclaration)
            yield return s;

        foreach (var argument in arguments)
        {
            var (expressionValueMethod, valueTrackerTypeName) = AttributeExpressionValue.RootSignature(argument.ArgumentType);
            var (expressionMethodName, nameTrackerTypeName) = AttributeExpressionName.RootSignature(argument.EntityTypeSymbol);

            var entityTypeName = argument.AnnotatedEntityType;
            var argumentTypeName = argument.AnnotatedArgumentType;

            var constructor = $"public {argument.ImplementationName}({MarshallerOptions.Name} {MarshallerOptions.ParamReference})"
                .CreateBlock($"{MarshallerOptions.FieldReference} = {MarshallerOptions.ParamReference};", $"{KeyMarshaller.PrimaryKeyMarshallerReference} = {KeyMarshaller.AssignmentRoot(argument.EntityTypeSymbol)};");
            var interfaceImplementation = constructor
                .Concat(Marshaller.RootSignature(argument.EntityTypeSymbol, entityTypeName))
                .Concat(Unmarshaller.RootSignature(argument.EntityTypeSymbol, entityTypeName))
                .Concat(KeyMarshaller.IndexKeyMarshallerRootSignature(argument.EntityTypeSymbol))
                .Concat(expressionValueMethod)
                .Append(expressionMethodName)
                .Append(KeyMarshaller.PrimaryKeyMarshallerDeclaration)
                .Prepend(MarshallerOptions.FieldDeclaration);

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{entityTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return options.TryInstantiate() switch
            {
                {} arg =>
                    $"public {Interface}<{entityTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}({arg});",
                null => $"public {Interface}<{entityTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName}({MarshallerOptions.Name} options) => new {argument.ImplementationName}(options);"
            };

            foreach (var s in classImplementation)
                yield return s;
        }
    }


    public static IEnumerable<string> CreateRepository(IEnumerable<DynamoDBMarshallerArguments> arguments, MarshallerOptions options)
    {
        var loadedArguments = arguments.ToArray();
        var getDynamoDbProperties = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, TypeExtensions.GetDynamoDbProperties);
        var code = CreateImplementations(loadedArguments, options)
            .Concat(Marshaller.CreateClass(loadedArguments, getDynamoDbProperties, options))
            .Concat(Unmarshaller.CreateClass(loadedArguments, getDynamoDbProperties, options))
            .Concat(AttributeExpressionName.CreateClasses(loadedArguments, getDynamoDbProperties, options))
            .Concat(AttributeExpressionValue.CreateExpressionAttributeValue(loadedArguments, getDynamoDbProperties, options))
            .Concat(KeyMarshaller.CreateKeys(loadedArguments, getDynamoDbProperties, options));

        return code;
    }
}