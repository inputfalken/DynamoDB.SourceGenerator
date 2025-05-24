using System.Runtime.CompilerServices;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{
    public static string ToCamelCaseFromPascal(this string str, [CallerMemberName] string? memberName = null)
    {
        var span = str.AsSpan();
        if (span.Length is 0)
            throw new ArgumentException($"Null or Empty string was provided from '{memberName}'");

        if (char.IsLower(span[0]))
            return str;

        Span<char> array = stackalloc char[span.Length];

        array[0] = char.ToLowerInvariant(span[0]);

        // Skip first element since we handled it manually.
        for (var i = 1; i < span.Length; i++)
            array[i] = span[i];

        return array.ToString();
    }

    public static string ToPrivateFieldFromPascal(this string str, [CallerMemberName] string? memberName = null)
    {
        var span = str.AsSpan();
        if (span.Length is 0)
            throw new ArgumentException($"Null or Empty string was provided from '{memberName}'");

        Span<char> array = stackalloc char[span.Length + 1];

        array[0] = '_';
        array[1] = char.ToLowerInvariant(span[0]);

        // Skip first element since we handled it manually.
        for (var i = 1; i < span.Length; i++)
            array[i + 1] = span[i];

        return array.ToString();
    }

    public static IEnumerable<string> ScopeTo(this IEnumerable<string> content, string header) =>
        CreateScope(header, content);

    public static IEnumerable<string> CreateScope(this string header, IEnumerable<string> content) => content
        .Select(x => $"    {x}")
        .Prepend("{")
        .Prepend(header)
        .Append("}");

    public static IEnumerable<string> CreateScope(this string header, string content)
    {
        yield return header;
        yield return "{";

        yield return $"    {content}";

        yield return "}";
    }

    public static IEnumerable<string> CreateScope(this string header, string content, string second)
    {
        yield return header;
        yield return "{";

        yield return $"    {content}";
        yield return $"    {second}";

        yield return "}";
    }

    public static string ToAlphaNumericMethodName(this string txt)
    {
        var index = 0;

        Span<char> arr = stackalloc char[txt.Length];
        foreach (var c in txt.AsSpan())
            if (char.IsLetter(c) || (index > 0 && char.IsNumber(c)))
                arr[index++] = c;

        return arr.Slice(0, index).ToString();
    }
}