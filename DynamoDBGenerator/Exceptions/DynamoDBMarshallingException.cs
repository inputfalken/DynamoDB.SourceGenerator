using System;
namespace DynamoDBGenerator.Exceptions;

public class DynamoDBMarshallingException : InvalidOperationException
{
    public DynamoDBMarshallingException(string memberName, string message) : base($"{message} (Data member '{memberName}')")
    {
        MemberName = memberName;
    }
    public string MemberName { get; }
}