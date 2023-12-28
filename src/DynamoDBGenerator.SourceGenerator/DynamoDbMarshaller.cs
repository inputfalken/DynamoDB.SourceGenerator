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

            var constructor = $"public {argument.ImplementationName}({MarshallerOptions.Name} options)".CreateBlock("Options = options;");
            var interfaceImplementation = constructor
                .Concat(Marshaller.RootSignature(argument.EntityTypeSymbol, entityTypeName))
                .Concat(Unmarshaller.RootSignature(argument.EntityTypeSymbol, entityTypeName))
                .Concat(KeyMarshaller.IndexKeyMarshaller(argument.EntityTypeSymbol))
                .Concat(expressionValueMethod)
                .Append(expressionMethodName)
                .Append(KeyMarshaller.PrimaryKeyMarshaller(argument.EntityTypeSymbol))
                .Prepend(MarshallerOptions.PropertyDeclaration);

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{entityTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return $"public {Interface}<{entityTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}({options.TryInstantiate()});";

            foreach (var s in classImplementation)
                yield return s;
        }
    }


    public static IEnumerable<string> CreateRepository(IEnumerable<DynamoDBMarshallerArguments> arguments, MarshallerOptions options)
    {
        var loadedArguments = arguments.ToArray();
        var getDynamoDbProperties = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, TypeExtensions.GetDynamoDbProperties);
        var code = CreateImplementations(loadedArguments, options)
            .Concat(Marshaller.CreateClass(loadedArguments, getDynamoDbProperties))
            .Concat(Unmarshaller.CreateClass(loadedArguments, getDynamoDbProperties, options))
            .Concat(AttributeExpressionName.CreateClasses(loadedArguments, getDynamoDbProperties))
            .Concat(AttributeExpressionValue.CreateExpressionAttributeValue(loadedArguments, getDynamoDbProperties))
            .Concat(KeyMarshaller.CreateKeys(loadedArguments, getDynamoDbProperties));

        return code;
    }
}