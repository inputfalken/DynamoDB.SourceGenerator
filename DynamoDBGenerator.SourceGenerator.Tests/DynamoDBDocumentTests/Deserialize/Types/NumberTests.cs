using Amazon.DynamoDBv2.Model;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Deserialize.Types;

[DynamoDBMarshaller(typeof(DecimalClass))]
[DynamoDBMarshaller(typeof(DoubleClass))]
[DynamoDBMarshaller(typeof(FloatClass))]
[DynamoDBMarshaller(typeof(Int16Class))]
[DynamoDBMarshaller(typeof(Int32Class))]
[DynamoDBMarshaller(typeof(Int64Class))]
[DynamoDBMarshaller(typeof(Int8Class))]
[DynamoDBMarshaller(typeof(UInt16Class))]
[DynamoDBMarshaller(typeof(UInt32Class))]
[DynamoDBMarshaller(typeof(UInt64Class))]
[DynamoDBMarshaller(typeof(UInt8Class))]
public partial class NumberTests
{

    [Fact]
    public void Deserialize_DecimalProperty_Included()
    {
        DecimalClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(DecimalClass.Count), new AttributeValue {N = "2.1"}}})
            .Should()
            .BeOfType<DecimalClass>()
            .Which
            .Count
            .Should()
            .Be(2.1m);
    }

    [Fact]
    public void Deserialize_DoubleProperty_Included()
    {
        DoubleClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(DoubleClass.Count), new AttributeValue {N = "2.1"}}})
            .Should()
            .BeOfType<DoubleClass>()
            .Which
            .Count
            .Should()
            .Be(2.1d);
    }

    [Fact]
    public void Deserialize_FloatProperty_Included()
    {
        FloatClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(FloatClass.Count), new AttributeValue {N = "2.1"}}})
            .Should()
            .BeOfType<FloatClass>()
            .Which
            .Count
            .Should()
            .Be(2.1f);
    }

    [Fact]
    public void Deserialize_In64Property_Included()
    {
        Int64ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int64Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<Int64Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_Int16Property_Included()
    {
        Int16ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int16Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<Int16Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_Int32Property_Included()
    {
        Int32ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int32Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<Int32Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_Int8Property_Included()
    {
        Int8ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(Int8Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<Int8Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_UIn64Property_Included()
    {
        UInt64ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(UInt64Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<UInt64Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_UInt16Property_Included()
    {
        UInt16ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(UInt16Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<UInt16Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }

    [Fact]
    public void Deserialize_UInt32Property_Included()
    {
        UInt32ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(UInt32Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<UInt32Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }
    [Fact]
    public void Deserialize_UInt8Property_Included()
    {
        UInt8ClassMarshaller
            .Unmarshall(new Dictionary<string, AttributeValue> {{nameof(UInt8Class.Count), new AttributeValue {N = "2"}}})
            .Should()
            .BeOfType<UInt8Class>()
            .Which
            .Count
            .Should()
            .Be(2);
    }
}

public class Int8Class
{
    [DynamoDBProperty]
    public sbyte Count { get; set; }
}

public class Int16Class
{
    [DynamoDBProperty]
    public short Count { get; set; }
}

public class Int32Class
{
    [DynamoDBProperty]
    public int Count { get; set; }
}

public class Int64Class
{
    [DynamoDBProperty]
    public long Count { get; set; }
}

public class UInt8Class
{
    [DynamoDBProperty]
    public byte Count { get; set; }
}

public class UInt16Class
{
    [DynamoDBProperty]
    public ushort Count { get; set; }
}

public class UInt32Class
{
    [DynamoDBProperty]
    public uint Count { get; set; }
}

public class UInt64Class
{
    [DynamoDBProperty]
    public ulong Count { get; set; }
}

public class DoubleClass
{
    [DynamoDBProperty]
    public double Count { get; set; }
}

public class DecimalClass
{
    [DynamoDBProperty]
    public decimal Count { get; set; }
}

public class FloatClass
{
    [DynamoDBProperty]
    public float Count { get; set; }
}