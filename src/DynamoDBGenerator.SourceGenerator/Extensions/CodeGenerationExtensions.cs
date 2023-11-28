using System.Diagnostics;
using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class CodeGenerationExtensions
{
    /// <summary>
    ///     Creates a namespace based on the type.
    /// </summary>
    public static IEnumerable<string> CreateNamespace(this ITypeSymbol type, IEnumerable<string> content)
    {
        var timestamp = Stopwatch.GetTimestamp();
        yield return $@"// <auto-generated | TimeStamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}>
#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using {Constants.AWSSDK_DynamoDBv2.Namespace.ModelFullName};
using {Constants.DynamoDBGenerator.Namespace.Root};
using {Constants.DynamoDBGenerator.Namespace.AttributesFullName};
using {Constants.DynamoDBGenerator.Namespace.ExceptionsFullName};
using {Constants.DynamoDBGenerator.Namespace.InternalFullName};";

        var nameSpace = type.ContainingNamespace.IsGlobalNamespace
            ? null
            : type.ContainingNamespace.ToString();
        if (nameSpace is not null)
        {
            foreach (var s in $"namespace {type.ContainingNamespace}".CreateBlock(content, 0))
                yield return s;
        }
        else
        {
            foreach (var s in content)
                yield return s;
        }
        var duration = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - timestamp);

        yield return $"// <auto-generated | Duration {duration.ToString()}>";
    }

    /// <summary>
    ///     Creates a class based on the type.
    /// </summary>
    public static IEnumerable<string> CreateClass(this ITypeSymbol type, IEnumerable<string> content, int indentLevel = 0)
    {
        return CreateClass(Accessibility.Public, type.Name, content, indentLevel, isPartial: true);
    }

    public static IEnumerable<string> CreateClass(in Accessibility accessibility, in string className, in string content, in int indentLevel, in bool isPartial = false)
    {
        return CreateClass(accessibility, className, Yield(content), indentLevel, isPartial);

        static IEnumerable<string> Yield(string item)
        {
            yield return item;
        }
    }

    public static IEnumerable<string> CreateClass(Accessibility accessibility, string className, IEnumerable<string> content, int indentLevel, bool isPartial = false)
    {
        return $"{accessibility.ToCode()} sealed{(isPartial ? " partial" : null)} class {className}".CreateBlock(content, indentLevel);

    }

    public static IEnumerable<string> CreateStruct(
        Accessibility accessibility,
        string structName,
        IEnumerable<string> content,
        int indentLevel,
        bool isPartial = false,
        bool isReadonly = false,
        bool isRecord = false)
    {
        return $"{accessibility.ToCode()}{(isPartial ? " partial" : null)}{(isReadonly ? " readonly" : null)}{(isRecord ? " record" : null)} struct {structName}".CreateBlock(content, indentLevel);
    }
}