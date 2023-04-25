namespace DynamoDBGenerator.SourceGenerator;

public class Constants
{
    public const string NewLine = @"
";

    public const string AttributeValueGeneratorMethodName = "BuildAttributeValues";
    public const string AttributeValueKeysGeneratorMethodName = "BuildAttributeValueKeys";

    public static class AccessModifiers
    {
        public const string Public = "public";
        public const string File = "file";
    }
}