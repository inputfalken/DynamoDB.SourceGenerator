using System;
using DynamoDBGenerator.Exceptions;
namespace DynamoDBGenerator.Internal;

/// <summary>
///     This is an internal API that supports the source generator and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new version.
/// </summary>
public static class ExceptionHelper
{
    public static DynamoDBMarshallingException NotNull(string memberName)
    {
        return new DynamoDBMarshallingException(memberName, "The data member is not supposed to be null, to allow this; make the data member nullable.");
    }

    public static DynamoDBMarshallingException KeysArgumentNotNull(string memberName, string argumentName)
    {
        return new DynamoDBMarshallingException(memberName, $"Argument '{argumentName}' can not be null.");
    }

    public static DynamoDBMarshallingException KeysInvalidConversion(string memberName, string argumentName, object value, string expectedType)
    {
        return new DynamoDBMarshallingException(memberName, $"Value '{{{value}}}' from argument '{{nameof({argumentName})}}' is not convertable to '{expectedType}'.");
    }

    public static InvalidOperationException KeysValueWithNoCorrespondence(string argumentName, object value)
    {
        return new InvalidOperationException($"Value '{value}' from argument '{argumentName}' was provided but there's no corresponding DynamoDBKeyAttribute.");
    }

    public static InvalidOperationException MissingDynamoDBAttribute(object? pkReference, object? rkReference)
    {
        return new InvalidOperationException($"Unable to create keys with the provided arguments (PartitionKey: {{{pkReference}}}, RangeKey: {{{rkReference}}}) due to missing DynamoDBKeyAttributes.");
    }

    public static InvalidOperationException NoDynamoDBAttributes(string typeName)
    {
        return new InvalidOperationException($"Could not create keys for type '{typeName}', include DynamoDBKeyAttribute on the correct properties.");
    }

    public static Exception ShouldNeverHappen()
    {
        return new Exception("Should never happen.");
    }

    public static ArgumentOutOfRangeException MissMatchedIndex(string paramName, string value)
    {
        return new ArgumentOutOfRangeException(paramName, $"Could not find any index match for value '{value}'.");
    }
}