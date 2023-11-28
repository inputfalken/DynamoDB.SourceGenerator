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
    public static IEnumerable<string> CreateBlock(this string header, IEnumerable<string> content, int indentLevel) => CreateBlockPrivate(header, content, indentLevel);

    public static IEnumerable<string> CreateBlock(this string header, string content, int indentLevel) => CreateBlockPrivate(header, content, indentLevel);

    private static IEnumerable<string> CreateBlockPrivate(string header, object content, int indentLevel)
    {
        if (indentLevel is 0)
        {
            yield return header;
            yield return "{";

            if (content is IEnumerable<string> enumerable)
                foreach (var s in enumerable)
                    yield return $"{Indent(1)}{s}";
            else
                yield return $"{Indent(1)}{content}";

            yield return "}";
        }
        else
        {
            var indent = Indent(indentLevel);

            yield return $"{indent}{header}";
            yield return string.Intern($"{indent}{{");

            if (content is IEnumerable<string> enumerable)
                foreach (var s in enumerable)
                    yield return $"{Indent(indentLevel + 1)}{s}";
            else
                yield return $"{Indent(indentLevel + 1)}{content}";

            yield return string.Intern($"{indent}}}");

        }

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

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }
}