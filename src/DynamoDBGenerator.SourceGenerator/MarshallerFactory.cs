using System.Diagnostics;
using DynamoDBGenerator.SourceGenerator.Extensions;
using DynamoDBGenerator.SourceGenerator.Generations;
using DynamoDBGenerator.SourceGenerator.Generations.Marshalling;
using DynamoDBGenerator.SourceGenerator.Types;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public static class MarshallerFactory
{
    private static IEnumerable<string> CreateImplementations(
        ITypeSymbol parentType,
        DynamoDBMarshallerArguments[] arguments,
        MarshallerOptions options)
    {
        return arguments.SelectMany(CollectionSelector);

        IEnumerable<string> CollectionSelector(DynamoDBMarshallerArguments argument)
        {
            var (expressionValueMethod, valueTrackerTypeName) =
                AttributeExpressionValue.RootSignature(parentType, argument.ArgumentType.TypeSymbol);
            var (expressionMethodName, nameTrackerTypeName) =
                AttributeExpressionName.RootSignature(parentType, argument.EntityTypeSymbol.TypeSymbol);
            var constructor =
                $"public {argument.ImplementationName}({options.FullName} {MarshallerOptions.ParamReference})"
                    .CreateScope($"{MarshallerOptions.FieldReference} = {MarshallerOptions.ParamReference};",
                        $"{Marshaller.KeyMarshaller.PrimaryKeyMarshallerReference} = {Marshaller.KeyMarshaller.AssignmentRoot(argument.EntityTypeSymbol.TypeSymbol)};");

            return constructor.Concat(Marshaller.RootSignature(argument.EntityTypeSymbol))
                .Concat(UnMarshaller.RootSignature(argument.EntityTypeSymbol))
                .Concat(Marshaller.KeyMarshaller.IndexKeyMarshallerRootSignature(argument.EntityTypeSymbol.TypeSymbol))
                .Concat(expressionValueMethod)
                .Append(expressionMethodName)
                .Append(Marshaller.KeyMarshaller.PrimaryKeyMarshallerDeclaration)
                .Prepend(options.FieldDeclaration)
                .ScopeTo(
                    $"file sealed class {argument.ImplementationName}: {Constants.DynamoDBGenerator.Marshaller.Interface}<{argument.EntityTypeSymbol.AnnotatedString}, {argument.ArgumentType.AnnotatedString}, {nameTrackerTypeName}, {valueTrackerTypeName}>");
        }
    }

    private static IEnumerable<string> PublicAccessibility(
        ITypeSymbol parentType,
        DynamoDBMarshallerArguments[] arguments,
        MarshallerOptions options
    )
    {
        return arguments
            .Select(x =>
            {
                var valueTrackerTypeName =
                    AttributeExpressionValue.GloballyAccessibleName(parentType, x.ArgumentType.TypeSymbol);
                var nameTrackerTypeName =
                    AttributeExpressionName.GloballyAccessibleName(parentType, x.EntityTypeSymbol.TypeSymbol);

                return options.TryInstantiate() switch
                {
                    { } y =>
                        $"public static {Constants.DynamoDBGenerator.Marshaller.Interface}<{x.EntityTypeSymbol.AnnotatedString}, {x.ArgumentType.AnnotatedString}, {nameTrackerTypeName}, {valueTrackerTypeName}> {x.AccessName} {{ get; }} = new {x.ImplementationName}({y});",
                    null =>
                        $"public static {Constants.DynamoDBGenerator.Marshaller.Interface}<{x.EntityTypeSymbol.AnnotatedString}, {x.ArgumentType.AnnotatedString}, {nameTrackerTypeName}, {valueTrackerTypeName}> {x.AccessName}({options.FullName} options) => new {x.ImplementationName}(options);"
                };
            });
    }

    public static IEnumerable<string> Create(
        INamedTypeSymbol originatingType,
        DynamoDBMarshallerArguments[] arguments,
        MarshallerOptions options
    )
    {
        var timestamp = Stopwatch.GetTimestamp();
        var dynamoDbProperties = TypeExtensions.CacheFactory(
            SymbolEqualityComparer.IncludeNullability,
            TypeExtensions.GetDynamoDbProperties
        );
        const string res = $@"#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using {Constants.AWSSDK_DynamoDBv2.Namespace.ModelFullName};
using {Constants.DynamoDBGenerator.Namespace.Root};
using {Constants.DynamoDBGenerator.Namespace.AttributesFullName};
using {Constants.DynamoDBGenerator.Namespace.ExceptionsFullName};
using {Constants.DynamoDBGenerator.Namespace.InternalFullName};";

        var marshaller = originatingType
            .NamespaceDeclaration(
                originatingType
                    .TypeDeclaration()
                    .CreateScope(
                        PublicAccessibility(originatingType, arguments, options)
                            .Concat(options.ClassDeclaration)
                            .Concat(AttributeExpressionName.CreateClasses(arguments, dynamoDbProperties, options))
                            .Concat(AttributeExpressionValue.CreateClasses(arguments, dynamoDbProperties, options))
                    )
                    .Concat(CreateImplementations(originatingType, arguments, options))
                    .Concat(Marshaller.CreateClass(arguments, dynamoDbProperties, options))
                    .Concat(UnMarshaller.CreateClass(arguments, dynamoDbProperties, options))
            )
            .Prepend(res);
        
        
        foreach (var x in marshaller)
            yield return x;
        // Need to walk through all the strings in order to capture the duration correctly.
        yield return
            $"// <auto-generated | Duration {TimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp)} | TimeStamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}>";
    }
}