namespace DynamoDBGenerator.SourceGenerator;

public static class Constants
{
    public static class DynamoDBGenerator
    {
        public static class Attribute
        {
            public const string DynamoDBMarshallerConstructor = "DynamoDBMarshallerConstructorAttribute";
            public const string DynamoDBMarshaller = "DynamoDBMarshallerAttribute";
            public const string DynamoDbMarshallerOptions = "DynamoDbMarshallerOptionsAttribute";

            public static class DynamoDBMarshallerArgument
            {

                public const string PropertyName = "PropertyName";
                public const string ArgumentType = "ArgumentType";
            }
            public static class DynamoDbMarshallerOptionsArgument
            {
                public const string Converters = "Converters";
                public const string EnumConversionStrategy = "EnumConversion";
                
                public static class ConversionStrategy
                {
                    public const int Integer = 1;
                    public const int Name = 2;
                    public const int NameCI = 3;
                    public const int LowerCase = 4;
                    public const int UpperCase = 5;
                }
            }
        }

        public const string AssemblyName = "DynamoDBGenerator";
        public const string DynamoDbDocumentPropertyFullname = $"{Namespace.AttributesFullName}.{Attribute.DynamoDBMarshaller}";

        public const string DynamoDBConverterFullName = $"{AssemblyName}.{Namespace.Options}.{Converter.AttributeValueConverters}";

        public static class Namespace
        {
            public const string Root = AssemblyName;
            public const string Attributes = "Attributes";
            public const string AttributesFullName = $"{AssemblyName}.{Attributes}";
            public const string InternalFullName = $"{AssemblyName}.Internal";
            public const string ExceptionsFullName = $"{AssemblyName}.Exceptions";
            public const string Converters = "Converters";
            public const string Options = "Options";
        }
        public static class Converter
        {
            public const string AttributeValueConverters = "AttributeValueConverters";
            public const string ReferenceType = "IReferenceTypeConverter";
            public const string ValueType = "IValueTypeConverter";
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

        
        public static class AttributeValueUtilityFactory
        {
            private const string ClassName = "AttributeValueUtilityFactory";
            public const string Null = $"{ClassName}.Null";
            public const string ToList = $"{ClassName}.ToList";
            public const string ToArray = $"{ClassName}.ToArray";
            public const string ToEnumerable = $"{ClassName}.ToEnumerable";
            public const string FromEnumerable = $"{ClassName}.FromEnumerable";
            public const string FromArray = $"{ClassName}.FromArray";
            public const string FromList = $"{ClassName}.FromList";
            public const string FromDictionary = $"{ClassName}.FromDictionary";
            public const string ToDictionary = $"{ClassName}.ToDictionary";

        }
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
            public const string Model = "Model";
            public const string ModelFullName = $"Amazon.DynamoDBv2.{Model}";
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