using Microsoft.CodeAnalysis;
namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class AccessibilityExtensions
{
    public static string ToCode(this Accessibility accessModifier)
    {
        return accessModifier switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Public => "public",
            Accessibility.NotApplicable => throw new NotSupportedException(),
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => throw new NotSupportedException(),
            _ => throw new ArgumentException(accessModifier.ToString())
        };
    }
}