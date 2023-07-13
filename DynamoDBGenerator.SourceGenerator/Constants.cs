using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public class Constants
{
    public const string DynamoDbDocumentFullName = nameof(DynamoDBGenerator) + "." + nameof(DynamoDbDocument);
    public const string DynamoDbDocumentPropertyFullname = nameof(DynamoDBGenerator) + "." + nameof(DynamoDbDocumentProperty);
    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;


}

public static class ConstantExtensions
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