using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Generations;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator;

public static class DynamoDbMarshaller
{
    private static IEnumerable<string> CreateImplementations(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        foreach (var argument in arguments)
        {
            var rootTypeName = argument.EntityTypeSymbol.Representation().annotated;
            var argumentTypeName = argument.ArgumentType.Representation().annotated;
            var (expressionValueMethod, valueTrackerTypeName) = AttributeExpressionValue.RootSignature(argument.ArgumentType);
            var (expressionMethodName, nameTrackerTypeName) = AttributeExpressionName.RootSignature(argument.EntityTypeSymbol);

            var interfaceImplementation = Marshaller.RootSignature(argument.EntityTypeSymbol, rootTypeName)
                .Concat(Unmarshaller.RootSignature(argument.EntityTypeSymbol, rootTypeName))
                .Concat(KeyMarshaller.IndexKeyMarshaller(argument.EntityTypeSymbol))
                .Concat(expressionValueMethod)
                .Append(expressionMethodName)
                .Append(KeyMarshaller.PrimaryKeyMarshaller(argument.EntityTypeSymbol));

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{rootTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return $"public {Interface}<{rootTypeName}, {argumentTypeName}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}();";

            foreach (var s in classImplementation)
                yield return s;

        }
    }


    public static IEnumerable<string> CreateRepository(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        var loadedArguments = arguments.ToArray();
        var getDynamoDbProperties = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, x => x.GetDynamoDbProperties());
        var code = CreateImplementations(loadedArguments)
            .Concat(Marshaller.CreateClass(loadedArguments, getDynamoDbProperties))
            .Concat(Unmarshaller.CreateClass(loadedArguments, getDynamoDbProperties))
            .Concat(AttributeExpressionName.CreateClasses(loadedArguments, getDynamoDbProperties))
            .Concat(AttributeExpressionValue.CreateExpressionAttributeValue(loadedArguments, getDynamoDbProperties))
            .Concat(KeyMarshaller.CreateKeys(loadedArguments, getDynamoDbProperties));

        return code;
    }
}