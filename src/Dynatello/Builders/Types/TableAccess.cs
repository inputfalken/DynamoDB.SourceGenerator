using DynamoDBGenerator;

namespace Dynatello.Builders.Types;

public readonly record struct TableAccess<T, TArg, TReferences, TArgumentReferences>
    where TReferences : IAttributeExpressionNameTracker
    where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    [Obsolete(Constants.ObsoleteConstructorMessage, true)]
    public TableAccess()
    {
        throw Constants.InvalidConstructor();
    }

    internal TableAccess(in string tableName, in IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }

    internal string TableName { get; }
    internal IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Item { get; }
}