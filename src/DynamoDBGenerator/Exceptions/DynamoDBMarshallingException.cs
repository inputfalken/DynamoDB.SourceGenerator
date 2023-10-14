using System;
namespace DynamoDBGenerator.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs during DynamoDB marshalling operations.
/// </summary>
public class DynamoDBMarshallingException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamoDBMarshallingException"/> class with the specified data member name and error message.
    /// </summary>
    /// <param name="memberName">The name of the data member associated with the exception.</param>
    /// <param name="message">A message that describes the error.</param>
    public DynamoDBMarshallingException(string memberName, string message) : base($"{message} (Data member '{memberName}')")
    {
        MemberName = memberName;
    }

    /// <summary>
    /// Gets the name of the data member associated with the exception.
    /// </summary>
    public string MemberName { get; }
}