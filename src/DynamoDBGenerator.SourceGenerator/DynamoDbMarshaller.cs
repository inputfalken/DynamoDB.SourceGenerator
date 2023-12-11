using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Generations;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;
using static DynamoDBGenerator.SourceGenerator.Constants.DynamoDBGenerator.Marshaller;
namespace DynamoDBGenerator.SourceGenerator;

public static class DynamoDbMarshaller
{
    internal static readonly Func<ITypeSymbol, (string annotated, string original)> TypeName;
    internal static readonly Func<ITypeSymbol, TypeIdentifier> TypeIdentifier;

    static DynamoDbMarshaller()
    {
        TypeName = TypeExtensions.GetTypeIdentifier(SymbolEqualityComparer.IncludeNullability);
        TypeIdentifier = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, x => x.GetKnownType());
    }

    private static IEnumerable<string> CreateImplementations(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        foreach (var argument in arguments)
        {
            var rootTypeName = TypeName(argument.EntityTypeSymbol).annotated;
            var valueTrackerTypeName = AttributeExpressionValue.TypeName(argument.ArgumentType);
            var nameTrackerTypeName = AttributeExpressionName.TypeName(argument.EntityTypeSymbol);

            var interfaceImplementation = Marshaller.RootSignature(argument.EntityTypeSymbol, rootTypeName)
                .Concat(Unmarshaller.RootSignature(argument.EntityTypeSymbol, rootTypeName))
                .Concat($"public {IndexKeyMarshallerInterface} IndexKeyMarshaller(string index)".CreateBlock(
                        "ArgumentNullException.ThrowIfNull(index);",
                        $"return new {Constants.DynamoDBGenerator.IndexKeyMarshallerImplementationTypeName}({KeyMarshaller.MethodName(argument.EntityTypeSymbol)}, index);"
                    )
                )
                .Concat($"public {valueTrackerTypeName} {AttributeExpressionValueTrackerMethodName}()".CreateBlock(
                        "var incrementer = new DynamoExpressionValueIncrementer();",
                        $"return new {valueTrackerTypeName}(incrementer.GetNext);"
                    )
                )
                .Append($"public {nameTrackerTypeName} {AttributeExpressionNameTrackerMethodName}() => new {nameTrackerTypeName}(null);")
                .Append($"public {KeyMarshallerInterface} PrimaryKeyMarshaller {{ get; }} = new {Constants.DynamoDBGenerator.KeyMarshallerImplementationTypeName}({KeyMarshaller.MethodName(argument.EntityTypeSymbol)});");

            var classImplementation = $"private sealed class {argument.ImplementationName}: {Interface}<{rootTypeName}, {TypeName(argument.ArgumentType).annotated}, {nameTrackerTypeName}, {valueTrackerTypeName}>"
                .CreateBlock(interfaceImplementation);

            yield return
                $"public {Interface}<{rootTypeName}, {TypeName(argument.ArgumentType).annotated}, {nameTrackerTypeName}, {valueTrackerTypeName}> {argument.PropertyName} {{ get; }} = new {argument.ImplementationName}();";

            foreach (var s in classImplementation)
                yield return s;

        }
    }


    public static IEnumerable<string> CreateRepository(IEnumerable<DynamoDBMarshallerArguments> arguments)
    {
        var loadedArguments = arguments.ToArray();
        var getDynamoDbProperties = TypeExtensions.CacheFactory(SymbolEqualityComparer.IncludeNullability, static x => x.GetDynamoDbProperties());
        var code = CreateImplementations(loadedArguments)
            .Concat(Marshaller.CreateClass(loadedArguments, getDynamoDbProperties))
            .Concat(Unmarshaller.CreateClass(loadedArguments, getDynamoDbProperties))
            .Concat(AttributeExpressionName.CreateClasses(loadedArguments, getDynamoDbProperties))
            .Concat(AttributeExpressionValue.CreateExpressionAttributeValue(loadedArguments, getDynamoDbProperties))
            .Concat(KeyMarshaller.CreateKeys(loadedArguments, getDynamoDbProperties));

        return code;
    }
}