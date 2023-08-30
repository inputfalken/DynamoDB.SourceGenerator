using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public class Constants
{
    public const string DynamoDbDocumentPropertyFullname = nameof(DynamoDBGenerator) + "." + nameof(DynamoDBDocumentAttribute);
    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;

    public const string NotNullErrorMessage = "The value is not supposed to be null, to allow this; make the property nullable.";

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