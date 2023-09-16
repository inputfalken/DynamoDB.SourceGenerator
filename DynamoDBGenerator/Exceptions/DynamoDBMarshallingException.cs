using System;
namespace DynamoDBGenerator.Exceptions;

public class DynamoDBMarshallingException : InvalidOperationException
{
    public string MemberName { get; }
    public DynamoDBMarshallingException(string memberName, string message) : base(message: $"{message} (Data member '{memberName}')")
    {
        MemberName = memberName;
    }
}