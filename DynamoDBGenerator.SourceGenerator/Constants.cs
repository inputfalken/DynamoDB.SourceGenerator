using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.Internal;

namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{
    public const string AssemblyName = nameof(DynamoDBGenerator);
    public const string AttributeNameSpace = nameof(Attributes);
    public const string MarshallerAttributeName = nameof(DynamoDBMarshallerAttribute);
    public const string MarshallerConstructorAttributeName = nameof(DynamoDBMarshallerConstructorAttribute);
    public const string DynamoDbDocumentPropertyFullname = $"{AssemblyName}.{AttributeNameSpace}.{MarshallerAttributeName}";
    public const string KeyMarshallerInterFaceName = nameof(IDynamoDBKeyMarshaller);
    public const string KeyMarshallerImplementationTypeName = nameof(DynamoDBKeyMarshallerDelegator);
    public const string IndexKeyMarshallerInterfaceName = nameof(IDynamoDBIndexKeyMarshaller);
    public const string IndexKeyMarshallerImplementationTypeName = nameof(IndexDynamoDBMarshallerDelegator);
    public const string NullExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.NotNull)}";
    public const string KeysArgumentNullExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.KeysArgumentNotNull)}";
    public const string KeysInvalidConversionExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.KeysInvalidConversion)}";
    public const string KeysValueWithNoCorrespondenceMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.KeysValueWithNoCorrespondence)}";
    public const string KeysMissingDynamoDBAttributeExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.MissingDynamoDBAttribute)}";
    public const string ShouldNeverHappenExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.ShouldNeverHappen)}";
    public const string MissMatchedIndexNameExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.MissMatchedIndex)}";
    public const string NoDynamoDBKeyAttributesExceptionMethod = $"{nameof(ExceptionHelper)}.{nameof(ExceptionHelper.NoDynamoDBAttributes)}";

    public const string NewLine = @"
";

    public const int MaxMethodNameLenght = 511;

}