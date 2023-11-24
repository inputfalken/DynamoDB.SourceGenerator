namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{

    public static class DynamoDBGenerator
    {
        public const string AssemblyName = "DynamoDBGenerator";
        public const string AttributeNameSpace = "Attributes";
        public const string MarshallerAttributeName = "DynamoDBMarshallerAttribute";
        public const string MarshallerConstructorAttributeName = "DynamoDBMarshallerConstructorAttribute";
        public const string MarshallerConstructorAttributePropertyName = "PropertyName";
        public const string MarshallerConstructorAttributeArgumentType = "ArgumentType";
        public const string DynamoDbDocumentPropertyFullname = $"{AssemblyName}.{AttributeNameSpace}.{MarshallerAttributeName}";

        public const string KeyMarshallerInterFaceName = "IDynamoDBKeyMarshaller";
        public const string KeyMarshallerImplementationTypeName = "DynamoDBKeyMarshallerDelegator";
        public const string IndexKeyMarshallerInterfaceName = "IDynamoDBIndexKeyMarshaller";
        public const string IndexKeyMarshallerImplementationTypeName = "IndexDynamoDBMarshallerDelegator";

        public const string NullExceptionMethod = $"{ExceptionHelperClass}.NotNull";
        public const string KeysArgumentNullExceptionMethod = $"{ExceptionHelperClass}.KeysArgumentNotNull";
        public const string KeysInvalidConversionExceptionMethod = $"{ExceptionHelperClass}.KeysInvalidConversion";
        public const string KeysValueWithNoCorrespondenceMethod = $"{ExceptionHelperClass}.KeysValueWithNoCorrespondence";
        public const string KeysMissingDynamoDBAttributeExceptionMethod = $"{ExceptionHelperClass}.MissingDynamoDBAttribute";
        public const string ShouldNeverHappenExceptionMethod = $"{ExceptionHelperClass}.ShouldNeverHappen";
        public const string MissMatchedIndexNameExceptionMethod = $"{ExceptionHelperClass}.MissMatchedIndex";
        public const string NoDynamoDBKeyAttributesExceptionMethod = $"{ExceptionHelperClass}.NoDynamoDBAttributes";

        private const string ExceptionHelperClass = "ExceptionHelper";

        public const string AttributeExpressionValueTracker = "IAttributeExpressionValueTracker";
        public const string AttributeExpressionInterfaceNameTracker = "IAttributeExpressionNameTracker";
        public const string AttributeExpressionNameTrackerMethodName = "AttributeExpressionNameTracker";
        public const string AttributeExpressionValueTrackerMethodName = "AttributeExpressionValueTracker";

        public const string AccessedNames = "AccessedNames";
        public const string AccessedValues = "AccessedValues";

        public const string DeserializeName = "Unmarshall";
        public const string InterfaceName = "IDynamoDBMarshaller";
        public const string SerializeName = "Marshall";
    }

    public static class DynamoDBAws
    {
        public const string AttributeValue = "AttributeValue";
        public const string AssemblyName = "AWSSDK.DynamoDBv2";
        public const string DynamoDBIgnoreAttribute = "DynamoDBIgnoreAttribute";

        public const string DynamoDBHashKeyAttribute = "DynamoDBHashKeyAttribute";
        public const string DynamoDBRangeKeyAttribute = "DynamoDBRangeKeyAttribute";
        public const string DynamoDBPropertyAttribute = "DynamoDBPropertyAttribute";

        public const string DynamoDBLocalSecondaryIndexRangeKeyAttribute = "DynamoDBLocalSecondaryIndexRangeKeyAttribute";

        public const string DynamoDBGlobalSecondaryIndexHashKeyAttribute = "DynamoDBGlobalSecondaryIndexHashKeyAttribute";
        public const string DynamoDBGlobalSecondaryIndexRangeKeyAttribute = "DynamoDBGlobalSecondaryIndexRangeKeyAttribute";


    }

    public const string NewLine = @"
";

}