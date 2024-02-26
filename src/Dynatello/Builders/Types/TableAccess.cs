using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct TableAccess<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    [Obsolete("Do not use this constructor!", true)]
    public TableAccess()
    {
        throw new InvalidOperationException("This is an invalid constructor access.");
    }

    internal TableAccess(in string tableName, in IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }

    internal string TableName { get; }
    internal IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Item { get; }
}