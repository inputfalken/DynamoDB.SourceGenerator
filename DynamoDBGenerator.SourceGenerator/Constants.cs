using DynamoDBGenerator.Attributes;
using Microsoft.CodeAnalysis;

namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{
    public const string AssemblyName = nameof(DynamoDBGenerator);
    public const string AttributeNameSpace = nameof(Attributes);
    public const string MarshallerAttributeName = nameof(DynamoDBMarshallerAttribute);
    public const string MarshallerConstructorAttributeName = nameof(DynamoDBMarshallerConstructorAttribute);
    public const string DynamoDbDocumentPropertyFullname = $"{AssemblyName}.{AttributeNameSpace}.{MarshallerAttributeName}";
    
    public static class Errors
    {
        
        
    }
    

    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;

    public const string NotNullErrorMessage = "The data member is not supposed to be null, to allow this; make the data member nullable.";
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