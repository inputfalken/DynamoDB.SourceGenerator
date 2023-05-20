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