namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{

    private static readonly IDictionary<int, string> IndentCache = new Dictionary<int, string>
    {
        {0, ""},
        {1, "    "},
        {2, "        "},
        {3, "            "},
        {4, "                "}
    };

    private static string Indent(int level)
    {
        if (IndentCache.TryGetValue(level, out var indent)) return indent;

        indent = new string(' ', level * 4);
        IndentCache[level] = indent;

        return indent;
    }
    public static IEnumerable<string> CreateScope(this string header, IEnumerable<string> content, int indentLevel)
    {
        var indent = Indent(indentLevel);

        yield return $"{indent}{header}";
        yield return string.Intern($"{indent}{{");

        foreach (var s in content)
            yield return $"{Indent(indentLevel + 1)}{s}";

        yield return string.Intern($"{indent}}}");
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