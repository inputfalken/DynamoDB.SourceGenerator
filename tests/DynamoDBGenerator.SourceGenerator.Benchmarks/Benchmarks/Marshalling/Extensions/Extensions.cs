using DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions.Types;

namespace DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.Extensions;

public static class Extensions
{
    public static DynamoDbMarshallerBenchmarkHelper<T, T2, T3, T4> ToBenchmarkHelper<T, T2, T3, T4>(
        this IDynamoDBMarshaller<T, T2, T3, T4> marshaller) where T3 : IAttributeExpressionNameTracker
        where T4 : IAttributeExpressionValueTracker<T2>
    {
        return new DynamoDbMarshallerBenchmarkHelper<T, T2, T3, T4>(marshaller);
    }
    
    public static AWSComparisonBenchmarkHelper<T, T2, T3, T4> ToAwsComparisonHelper<T, T2, T3, T4>(
        this IDynamoDBMarshaller<T, T2, T3, T4> marshaller) where T3 : IAttributeExpressionNameTracker
        where T4 : IAttributeExpressionValueTracker<T2>
    {
        return new AWSComparisonBenchmarkHelper<T, T2, T3, T4>(marshaller);
    }
}