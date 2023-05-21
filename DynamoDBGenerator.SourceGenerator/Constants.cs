namespace DynamoDBGenerator.SourceGenerator;

public class Constants
{
    public const string AttributeValueGeneratorFullName =
        nameof(DynamoDBGenerator) + "." + nameof(AttributeValueGeneratorAttribute);

    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;

    // ReSharper disable once InconsistentNaming
    public const string DynamoDBPutOperationFullName =
        nameof(DynamoDBGenerator) + "." + nameof(DynamoDBPutOperationAttribute);
    // ReSharper disable once InconsistentNaming
    public const string DynamoDBUpdateOperationFullName =
        nameof(DynamoDBGenerator) + "." + nameof(DynamoDBUpdateOperationAttribute);

    public enum AccessModifier
    {
        Public = 1,
        Private = 2,
        Protected = 3
    }
}

public static class ConstantExtensions
{
    public static string ToCode(this Constants.AccessModifier accessModifier)
    {
        return accessModifier switch
        {
            Constants.AccessModifier.Private => "private",
            Constants.AccessModifier.Protected => "protected",
            Constants.AccessModifier.Public => "public",
            _ => throw new ArgumentException(accessModifier.ToString())
        };
    }
}