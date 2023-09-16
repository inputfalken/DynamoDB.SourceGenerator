using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;

namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{
    public const string AssemblyName = nameof(DynamoDBGenerator);
    public const string AttributeNameSpace = nameof(Attributes);
    public const string MarshallerAttributeName = nameof(DynamoDBMarshallerAttribute);
    public const string MarshallerConstructorAttributeName = nameof(DynamoDBMarshallerConstructorAttribute);
    public const string DynamoDbDocumentPropertyFullname = $"{AssemblyName}.{AttributeNameSpace}.{MarshallerAttributeName}";
    public const string MarshallingExceptionName = nameof(DynamoDBMarshallingException);

    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;

    public const string NotNullErrorMessage = "The data member is not supposed to be null, to allow this; make the data member nullable.";
}