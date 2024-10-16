namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{
    public static string ToCamelCaseFromPascal(this string str, [System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        return ToCamelCaseFromPascal(str.AsSpan(), memberName).ToString();
    }

    public static string ToPrivateFieldFromPascal(this string str, [System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        return ToPrivateFieldFromPascal(str.AsSpan(), memberName).ToString();
    }

    public static ReadOnlySpan<char> ToPrivateFieldFromPascal(this ReadOnlySpan<char> span, [System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        if (span.Length is 0)
            throw new ArgumentException($"Null or Empty string was provided from '{memberName}'");

        var array = new char[span.Length + 1];

        array[0] = '_';
        array[1] = Char.ToLowerInvariant(span[0]);

        // Skip first element since we handled it manually.
        for (var i = 1; i < span.Length; i++)
            array[i + 1] = span[i];

        return array;
    }
    public static ReadOnlySpan<char> ToCamelCaseFromPascal(this ReadOnlySpan<char> span, [System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        if (span.Length is 0)
            throw new ArgumentException($"Null or Empty string was provided from '{memberName}'");

        if (char.IsLower(span[0]))
            return span;

        var array = new char[span.Length];

        array[0] = Char.ToLowerInvariant(span[0]);

        // Skip first element since we handled it manually.
        for (var i = 1; i < span.Length; i++)
            array[i] = span[i];

        return array;
    }

    public static IEnumerable<string> ScopeTo(this IEnumerable<string> content, string header)
    {
        return CreateScope(header, content);
    }

    public static IEnumerable<string> CreateScope(this string header, IEnumerable<string> content)
    {
        yield return header;
        yield return "{";

        foreach (var s in content)
            yield return $"    {s}";

        yield return "}";
    }

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
        var arr = new char[txt.Length];
        var index = 0;

        for (var i = 0; i < txt.Length; i++)
        {
            var c = txt[i];
            if (char.IsLetter(c) || index > 0 && char.IsNumber(c))
            {
                arr[index++] = c;
            }
        }

        return new string(arr, 0, index);
    }
}
