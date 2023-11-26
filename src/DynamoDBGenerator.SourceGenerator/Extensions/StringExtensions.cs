namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{

    private static readonly IDictionary<int, string> IndentCache = new Dictionary<int, string>();

    public static string Indent(int level)
    {
        var @base = level switch
        {
            
            1 => "    ",
            2 => "        ",
            3 => "            ",
            4 => "                ",
            _ => null
        };

        if (@base is not null)
            return @base;

        if (IndentCache.TryGetValue(level, out var indent)) return indent;
        
        indent = new string(' ', level * 4);
        IndentCache[level] = indent;

        return indent;
    }

    public static string ToAlphaNumericMethodName(this string txt)
    {
        var arr = new char[txt.Length];
        var index = 0;

        for (var i = 0; i < txt.Length; i++)
        {
            var c = txt[i];
            if (char.IsLetter(c) || (index > 0 && char.IsNumber(c)))
            {
                arr[index++] = c;
            }
        }

        return new string(arr, 0, index);
    }
}