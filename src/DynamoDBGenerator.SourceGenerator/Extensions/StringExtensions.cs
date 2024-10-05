namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{
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
