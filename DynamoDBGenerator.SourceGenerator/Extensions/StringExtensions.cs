using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class StringExtensions
{
    public static string ToBase64(this string txt)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(txt);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string ToAlphaNumericMethodName(this string txt)
    {
        var arr = txt
            .Select((x, y) => (x, y))
            .Where(x => char.IsLetter(x.x) || (x.y > 0 && char.IsNumber(x.x)))
            .Select(x => x.x)
            .ToArray();

        return new string(arr);

    }
}