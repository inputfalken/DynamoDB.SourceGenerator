namespace DynamoDBGenerator.SourceGenerator.Tests.AttributeValueConversion.Types;

public class NumberTests
{
    [Fact]
    public void BuildAttributeValues_UInt8Property_Included()
    {
        const int count = 2;
        var @class = new UInt16Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt8Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_UInt16Property_Included()
    {
        const int count = 2;
        var @class = new UInt16Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt16Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_UInt32Property_Included()
    {
        const int count = 2;
        var @class = new UInt32Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt32Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_UIn64Property_Included()
    {
        const int count = 2;
        var @class = new UInt64Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(UInt64Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_Int8Property_Included()
    {
        const int count = 2;
        var @class = new Int16Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int8Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_Int16Property_Included()
    {
        const int count = 2;
        var @class = new Int16Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int16Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_Int32Property_Included()
    {
        const int count = 2;
        var @class = new Int32Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int32Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_In64Property_Included()
    {
        const int count = 2;
        var @class = new Int64Class
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(Int64Class.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_DoubleProperty_Included()
    {
        const double count = 2.1d;
        var @class = new DoubleClass
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DoubleClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_FloatProperty_Included()
    {
        const float count = 2.1f;
        var @class = new FloatClass()
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(FloatClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }

    [Fact]
    public void BuildAttributeValues_DecimalProperty_Included()
    {
        const decimal count = 2.1m;
        var @class = new DecimalClass()
        {
            Count = count
        };

        var result = @class.BuildAttributeValues();

        result
            .Should()
            .NotBeEmpty()
            .And
            .ContainKey(nameof(DecimalClass.Count))
            .And
            .ContainSingle(x => x.Value.N == count.ToString());
    }
}

[AttributeValueGenerator]
public partial class Int8Class
{
    [DynamoDBProperty]
    public sbyte Count { get; set; }
}

[AttributeValueGenerator]
public partial class Int16Class
{
    [DynamoDBProperty]
    public short Count { get; set; }
}

[AttributeValueGenerator]
public partial class Int32Class
{
    [DynamoDBProperty]
    public int Count { get; set; }
}

[AttributeValueGenerator]
public partial class Int64Class
{
    [DynamoDBProperty]
    public long Count { get; set; }
}

[AttributeValueGenerator]
public partial class UInt8Class
{
    [DynamoDBProperty]
    public byte Count { get; set; }
}

[AttributeValueGenerator]
public partial class UInt16Class
{
    [DynamoDBProperty]
    public ushort Count { get; set; }
}

[AttributeValueGenerator]
public partial class UInt32Class
{
    [DynamoDBProperty]
    public uint Count { get; set; }
}

[AttributeValueGenerator]
public partial class UInt64Class
{
    [DynamoDBProperty]
    public ulong Count { get; set; }
}

[AttributeValueGenerator]
public partial class DoubleClass
{
    [DynamoDBProperty]
    public double Count { get; set; }
}

[AttributeValueGenerator]
public partial class DecimalClass
{
    [DynamoDBProperty]
    public decimal Count { get; set; }
}

[AttributeValueGenerator]
public partial class FloatClass
{
    [DynamoDBProperty]
    public float Count { get; set; }
}