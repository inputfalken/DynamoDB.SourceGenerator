namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{
    public static class DynamoDBGenerator
    {
        public static class Attribute
        {
            public const string DynamoDBMarshallerConstructor = "DynamoDBMarshallerConstructorAttribute";
            public const string DynamoDBMarshaller = "DynamoDBMarshallerAttribute";

            public static class DynamoDBMarshallerArgument
            {

                public const string PropertyName = "PropertyName";
                public const string ArgumentType = "ArgumentType";
            }
        }

        public const string AssemblyName = "DynamoDBGenerator";
        public const string DynamoDbDocumentPropertyFullname = $"{Namespace.Attributes}.{Attribute.DynamoDBMarshaller}";

        public static class Namespace
        {
            public const string Root = AssemblyName;
            public const string Attributes = $"{AssemblyName}.Attributes";
            public const string Internal = $"{AssemblyName}.Internal";
            public const string Exceptions = $"{AssemblyName}.Exceptions";
        }

        public static class Marshaller
        {
            public const string KeyMarshallerInterface = "IDynamoDBKeyMarshaller";
            public const string Interface = "IDynamoDBMarshaller";
            public const string IndexKeyMarshallerInterface = "IDynamoDBIndexKeyMarshaller";

            public const string AttributeExpressionNameTrackerInterface = "IAttributeExpressionNameTracker";
            public const string AttributeExpressionNameTrackerMethodName = "AttributeExpressionNameTracker";

            public const string AttributeExpressionValueTrackerInterface = "IAttributeExpressionValueTracker";
            public const string AttributeExpressionValueTrackerMethodName = "AttributeExpressionValueTracker";

            public const string AttributeExpressionNameTrackerInterfaceAccessedNames = "AccessedNames";
            public const string AttributeExpressionValueTrackerAccessedValues = "AccessedValues";

            public const string UnmarshalMethodName = "Unmarshall";
            public const string MarshallMethodName = "Marshall";
        }


        public const string KeyMarshallerImplementationTypeName = "DynamoDBKeyMarshallerDelegator";
        public const string IndexKeyMarshallerImplementationTypeName = "IndexDynamoDBMarshallerDelegator";

        public static class ExceptionHelper
        {
            private const string ExceptionHelperClass = "ExceptionHelper";
            public const string NullExceptionMethod = $"{ExceptionHelperClass}.NotNull";
            public const string KeysArgumentNullExceptionMethod = $"{ExceptionHelperClass}.KeysArgumentNotNull";
            public const string KeysInvalidConversionExceptionMethod = $"{ExceptionHelperClass}.KeysInvalidConversion";
            public const string KeysValueWithNoCorrespondenceMethod = $"{ExceptionHelperClass}.KeysValueWithNoCorrespondence";
            public const string KeysMissingDynamoDBAttributeExceptionMethod = $"{ExceptionHelperClass}.MissingDynamoDBAttribute";
            public const string ShouldNeverHappenExceptionMethod = $"{ExceptionHelperClass}.ShouldNeverHappen";
            public const string MissMatchedIndexNameExceptionMethod = $"{ExceptionHelperClass}.MissMatchedIndex";
            public const string NoDynamoDBKeyAttributesExceptionMethod = $"{ExceptionHelperClass}.NoDynamoDBAttributes";
        }


    }

    // ReSharper disable once IdentifierTypo
    // ReSharper disable once InconsistentNaming
    public static class AWSSDK_DynamoDBv2
    {
        public const string AttributeValue = "AttributeValue";
        public const string AssemblyName = "AWSSDK.DynamoDBv2";

        public static class Namespace
        {
            public const string Model = "Amazon.DynamoDBv2.Model";
        }

        public static class Attribute
        {
            public const string DynamoDBIgnore = "DynamoDBIgnoreAttribute";
            public const string DynamoDBHashKey = "DynamoDBHashKeyAttribute";
            public const string DynamoDBRangeKey = "DynamoDBRangeKeyAttribute";
            public const string DynamoDBProperty = "DynamoDBPropertyAttribute";
            public const string DynamoDBLocalSecondaryIndexRangeKey = "DynamoDBLocalSecondaryIndexRangeKeyAttribute";
            public const string DynamoDBGlobalSecondaryIndexHashKey = "DynamoDBGlobalSecondaryIndexHashKeyAttribute";
            public const string DynamoDBGlobalSecondaryIndexRangeKey = "DynamoDBGlobalSecondaryIndexRangeKeyAttribute";
        }

    }

    public const string NewLine = @"
";
}