namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Serialize.Types;

[DynamoDBDocument(typeof(DecimalClass))]
[DynamoDBDocument(typeof(DoubleClass))]
[DynamoDBDocument(typeof(FloatClass))]
[DynamoDBDocument(typeof(Int16Class))]
[DynamoDBDocument(typeof(Int32Class))]
[DynamoDBDocument(typeof(Int64Class))]
[DynamoDBDocument(typeof(Int8Class))]
[DynamoDBDocument(typeof(UInt16Class))]
[DynamoDBDocument(typeof(UInt32Class))]
[DynamoDBDocument(typeof(UInt64Class))]
[DynamoDBDocument(typeof(UInt8Class))]
public partial class NumberTests
{

    [Fact]
    public void Serialize_DecimalProperty_Included()
    {
        const decimal count = 2.1m;
        var @class = new DecimalClass
        {
            Count = count
        };

        DecimalClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DecimalClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_DoubleProperty_Included()
    {
        const double count = 2.1d;
        var @class = new DoubleClass
        {
            Count = count
        };

        DoubleClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DoubleClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_FloatProperty_Included()
    {
        const float count = 2.1f;
        var @class = new FloatClass
        {
            Count = count
        };

        FloatClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(FloatClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_In64Property_Included()
    {
        const int count = 2;
        var @class = new Int64Class
        {
            Count = count
        };

        Int64ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int64Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_Int16Property_Included()
    {
        const int count = 2;
        var @class = new Int16Class
        {
            Count = count
        };

        Int16ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int16Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_Int32Property_Included()
    {
        const int count = 2;
        var @class = new Int32Class
        {
            Count = count
        };

        Int32ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_Int8Property_Included()
    {
        const int count = 2;
        var @class = new Int8Class
        {
            Count = count
        };

        Int8ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int8Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_UIn64Property_Included()
    {
        const int count = 2;
        var @class = new UInt64Class
        {
            Count = count
        };

        UInt64ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt64Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_UInt16Property_Included()
    {
        const int count = 2;
        var @class = new UInt16Class
        {
            Count = count
        };

        UInt16ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt16Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void Serialize_UInt32Property_Included()
    {
        const int count = 2;
        var @class = new UInt32Class
        {
            Count = count
        };

        UInt32ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt32Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }
    [Fact]
    public void Serialize_UInt8Property_Included()
    {
        const int count = 2;
        var @class = new UInt8Class
        {
            Count = count
        };

        UInt8ClassDocument
            .Serialize(@class)
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt8Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
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
