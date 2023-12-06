using Amazon.DynamoDBv2.Model;
using DynamoDBGenerator.Attributes;
namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Types;

[DynamoDBMarshaller(typeof(SByte))]
[DynamoDBMarshaller(typeof(Short))]
[DynamoDBMarshaller(typeof(Int))]
[DynamoDBMarshaller(typeof(Long))]
[DynamoDBMarshaller(typeof(Byte))]
[DynamoDBMarshaller(typeof(UShort))]
[DynamoDBMarshaller(typeof(UInt))]
[DynamoDBMarshaller(typeof(ULong))]
public partial class IntegerTests
{

    private static readonly SByte SByteDto = new(30);

    private static readonly Dictionary<string, AttributeValue> SByteAttributeValues = new()
    {
        {nameof(SByte.SByteValue), new AttributeValue {N = SByteDto.SByteValue.ToString()}}
    };

    [Fact]
    public void Marshal_SByte()
    {
        SByteMarshaller.Marshall(SByteDto).Should().BeEquivalentTo(SByteAttributeValues);
    }

    [Fact]
    public void Unmarshal_SByte()
    {
        SByteMarshaller.Unmarshall(SByteAttributeValues).Should().BeEquivalentTo(SByteDto);
    }

    public record SByte(sbyte SByteValue);

    private static readonly Short ShortDto = new(31);

    private static readonly Dictionary<string, AttributeValue> ShortAttributeValues = new()
    {
        {nameof(Short.ShortValue), new AttributeValue {N = ShortDto.ShortValue.ToString()}}
    };

    [Fact]
    public void Marshal_Short()
    {
        ShortMarshaller.Marshall(ShortDto).Should().BeEquivalentTo(ShortAttributeValues);
    }

    [Fact]
    public void Unmarshal_Short()
    {
        ShortMarshaller.Unmarshall(ShortAttributeValues).Should().BeEquivalentTo(ShortDto);
    }

    public record Short(short ShortValue);

    private static readonly Int IntDto = new(32);

    private static readonly Dictionary<string, AttributeValue> IntAttributeValues = new()
    {
        {nameof(Int.IntValue), new AttributeValue {N = IntDto.IntValue.ToString()}}
    };

    [Fact]
    public void Marshal_Int()
    {
        IntMarshaller.Marshall(IntDto).Should().BeEquivalentTo(IntAttributeValues);
    }

    [Fact]
    public void Unmarshal_Int()
    {
        IntMarshaller.Unmarshall(IntAttributeValues).Should().BeEquivalentTo(IntDto);
    }

    public record Int(int IntValue);

    private static readonly Long LongDto = new(33);

    private static readonly Dictionary<string, AttributeValue> LongAttributeValues = new()
    {
        {nameof(Long.LongValue), new AttributeValue {N = LongDto.LongValue.ToString()}}
    };

    [Fact]
    public void Marshal_Long()
    {
        LongMarshaller.Marshall(LongDto).Should().BeEquivalentTo(LongAttributeValues);
    }

    [Fact]
    public void Unmarshal_Long()
    {
        LongMarshaller.Unmarshall(LongAttributeValues).Should().BeEquivalentTo(LongDto);
    }

    public record Long(long LongValue);

    private static readonly Byte ByteDto = new(34);

    private static readonly Dictionary<string, AttributeValue> ByteAttributeValues = new()
    {
        {nameof(Byte.ByteValue), new AttributeValue {N = ByteDto.ByteValue.ToString()}}
    };


    [Fact]
    public void Marshal_Byte()
    {
        ByteMarshaller.Marshall(ByteDto).Should().BeEquivalentTo(ByteAttributeValues);
    }

    [Fact]
    public void Unmarshal_Byte()
    {
        ByteMarshaller.Unmarshall(ByteAttributeValues).Should().BeEquivalentTo(ByteDto);
    }

    public record Byte(byte ByteValue);


    private static readonly UShort UShortDto = new(35);

    private static readonly Dictionary<string, AttributeValue> UShortAttributeValues = new()
    {
        {nameof(UShort.UShortValue), new AttributeValue {N = UShortDto.UShortValue.ToString()}}
    };

    [Fact]
    public void Marshal_UShort()
    {
        UShortMarshaller.Marshall(UShortDto).Should().BeEquivalentTo(UShortAttributeValues);
    }

    [Fact]
    public void Unmarshal_Ushort()
    {
        UShortMarshaller.Unmarshall(UShortAttributeValues).Should().BeEquivalentTo(UShortDto);
    }

    public record UShort(ushort UShortValue);


    private static readonly UInt UIntDto = new(36);

    private static readonly Dictionary<string, AttributeValue> UIntAttributeValues = new()
    {
        {nameof(UIntDto.UIntValue), new AttributeValue {N = UIntDto.UIntValue.ToString()}}
    };

    [Fact]
    public void Marshal_UInt()
    {
        UIntMarshaller.Marshall(UIntDto).Should().BeEquivalentTo(UIntAttributeValues);
    }

    [Fact]
    public void Unmarshal_UInt()
    {
        UIntMarshaller.Unmarshall(UIntAttributeValues).Should().BeEquivalentTo(UIntDto);
    }

    public record UInt(uint UIntValue);

    private static readonly ULong ULongDto = new(37);

    private static readonly Dictionary<string, AttributeValue> ULongAttributeValues = new()
    {
        {nameof(ULong.ULongValue), new AttributeValue {N = ULongDto.ULongValue.ToString()}}
    };

    [Fact]
    public void Marshal_ULong()
    {
        ULongMarshaller.Marshall(ULongDto).Should().BeEquivalentTo(ULongAttributeValues);
    }

    [Fact]
    public void Unmarshal_ULong()
    {
        ULongMarshaller.Unmarshall(ULongAttributeValues).Should().BeEquivalentTo(ULongDto);
    }

    public record ULong(ulong ULongValue);
}