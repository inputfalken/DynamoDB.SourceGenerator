namespace DynamoDBGenerator.SourceGenerator.Extensions;

public static class EnumerableExtensions
{

    public static IEnumerable<TResult> DefaultAndLast<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> @default, Func<T, TResult> onLast)
    {
        var buffer = default(T);
        var isBuffered = false;

        foreach (var item in enumerable)
        {
            if (isBuffered)
                yield return @default(buffer!);

            isBuffered = true;
            buffer = item;
        }

        if (isBuffered)
        {
            yield return onLast(buffer!);
        }
    }

}