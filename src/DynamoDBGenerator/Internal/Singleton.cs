namespace DynamoDBGenerator.Internal;

internal interface IStaticSingleton<out T> where T : new()
{
    private static T? _backingField;
    internal static virtual T Singleton => _backingField ??= new T();
}

internal static class Singleton
{
    public static T Static<T>() where T : IStaticSingleton<T>, new() => T.Singleton;
}