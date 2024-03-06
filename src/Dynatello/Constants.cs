namespace Dynatello;

internal static class Constants
{
    internal const string ObsoleteConstructorMessage = "Do not use this constructor!";
    private const string ConstructorException = "This is an invalid constructor access.";

    internal static Exception InvalidConstructor()
    {
        return new InvalidOperationException(ConstructorException);
    }
    
}