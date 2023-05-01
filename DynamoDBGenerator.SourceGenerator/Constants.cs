namespace DynamoDBGenerator.SourceGenerator;

public class Constants
{
    public const string NewLine = @"
";

    public const string AttributeValueGeneratorMethodName = nameof(AttributeValueGeneratorAttribute) 
                                                            + "_" 
                                                            + nameof(DynamoDBGenerator) 
                                                            + "_" 
                                                            + nameof(SourceGenerator);

}