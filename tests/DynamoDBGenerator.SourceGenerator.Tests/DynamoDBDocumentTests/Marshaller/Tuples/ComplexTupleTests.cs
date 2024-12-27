using Amazon.DynamoDBv2.Model;
using AutoFixture;
using DynamoDBGenerator.Attributes;
using DynamoDBGenerator.Exceptions;
using DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Asserters;

namespace DynamoDBGenerator.SourceGenerator.Tests.DynamoDBDocumentTests.Marshaller.Tuples;

[DynamoDBMarshaller(EntityType =
    typeof((string FirstName, int Age,
        IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
        PhoneAndEmail )))]
public partial class ComplexTupleTests : MarshalAsserter<(string FirstName, int Age,
    IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
    PhoneAndEmail)>
{
    private static readonly Fixture Fixture = new();

    public static IEnumerable<object[]> InvalidData
    {
        get
        {
            yield return new object[]
            {
                "FirstName",
                (
                    null as string,
                    2,
                    Array.Empty<((string Address, string? ZipCode), (string Email, string Phone)?)>()
                )
            };

            yield return new object[]
            {
                "Address",
                (
                    "Bob",
                    2,
                    new ((string? Address, string? ZipCode), (string Email, string Phone)?)[]
                    {
                        ((null, "ZIP"), ("EMAIL", "+PHONE"))
                    }
                )
            };

            yield return new object[]
            {
                "FirstName",
                (
                    null as string,
                    2,
                    new ((string? Address, string? ZipCode), (string Email, string Phone)?)[]
                    {
                        ((null, "ZIP"), ("EMAIL", "+PHONE"))
                    }
                )
            };

            yield return new object[]
            {
                "Email",
                (
                    "Bob",
                    2,
                    new ((string Address, string? ZipCode), (string? Email, string Phone)?)[]
                    {
                        (("ADDRESS", "ZIP"), (null, "PHONE"))
                    }
                )
            };

            yield return new object[]
            {
                "Phone",
                (
                    "Bob",
                    2,
                    new ((string Address, string? ZipCode), (string Email, string? Phone)?)[]
                    {
                        (("ADDRESS", "ZIP"), ("EMAIL", null))
                    }
                )
            };
        }
    }

    private static ((string FirstName, int Age,
        IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
        PhoneAndEmail)
        tuple, Dictionary<string, AttributeValue> dict) Av(
            (string FirstName, int Age,
                IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
                PhoneAndEmail) tuple
        )
    {
        var dict = new Dictionary<string, AttributeValue>
        {
            { nameof(tuple.FirstName), new AttributeValue { S = tuple.FirstName } },
            { nameof(tuple.Age), new AttributeValue { N = tuple.Age.ToString() } },
            {
                nameof(tuple.PhoneAndEmail),
                new AttributeValue { L = tuple.PhoneAndEmail.Select(BuildPhoneAndMail).ToList() }
            }
        };

        return (tuple, dict);

        static AttributeValue BuildPhoneAndMail(
            ((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums) valueTuple)
        {
            var attributeValue = new AttributeValue();

            attributeValue.M.Add(nameof(valueTuple.Address), new AttributeValue
            {
                M = valueTuple.Address.ZipCode is null
                    ? new Dictionary<string, AttributeValue>
                    {
                        { nameof(valueTuple.Address.Address), new AttributeValue { S = valueTuple.Address.Address } }
                    }
                    : new Dictionary<string, AttributeValue>
                    {
                        { nameof(valueTuple.Address.Address), new AttributeValue { S = valueTuple.Address.Address } },
                        { nameof(valueTuple.Address.ZipCode), new AttributeValue { S = valueTuple.Address.ZipCode } }
                    }
            });

            if (valueTuple.Mediums is { } mediums)
                attributeValue.M.Add(nameof(valueTuple.Mediums), new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { nameof(mediums.Email), new AttributeValue { S = mediums.Email } },
                        { nameof(mediums.Phone), new AttributeValue { S = mediums.Phone } }
                    }
                });

            return attributeValue;
        }
    }

    protected override IEnumerable<((string FirstName, int Age,
        IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
        PhoneAndEmail)
        element, Dictionary<string, AttributeValue>
        attributeValues)> Arguments()
    {
        var customizationComposer =
            Fixture
                .Build<(string FirstName, int Age,
                    IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)?
                        Mediums)>
                    PhoneAndEmail)>();

        yield return Av(customizationComposer.Create());

        yield return Av(
            customizationComposer.Create() switch
            {
                var a => a with { PhoneAndEmail = a.PhoneAndEmail.Select(y => y with { Mediums = null }).ToArray() }
            }
        );

        yield return Av(
            customizationComposer.Create() switch
            {
                var a => a with
                {
                    PhoneAndEmail = a.PhoneAndEmail.Select(x => x with { Address = x.Address with { ZipCode = null } })
                        .ToArray()
                }
            }
        );
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public void Unmarshall_NullField_Throws(
        string dataMember,
        (string FirstName, int Age,
            ((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)[] PhoneAndEmail
            ) arg)
    {
        var (_, dict) = Av(arg);
        var act = () => ValueTupleMarshaller.Unmarshall(dict);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(dataMember);
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public void Marshall_NullField_Throws(
        string dataMember,
        (string FirstName, int Age,
            ((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)[] PhoneAndEmail
            ) arg)
    {
        var (e, _) = Av(arg);
        var act = () => ValueTupleMarshaller.Marshall(e);
        act.Should().Throw<DynamoDBMarshallingException>().Which.MemberName.Should().Be(dataMember);
    }

    protected override (string FirstName, int Age,
        IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
        PhoneAndEmail)
        UnmarshallImplementation(
            Dictionary<string, AttributeValue> attributeValues)
    {
        return ValueTupleMarshaller.Unmarshall(attributeValues);
    }

    protected override Dictionary<string, AttributeValue> MarshallImplementation(
        (string FirstName, int Age,
            IReadOnlyCollection<((string Address, string? ZipCode) Address, (string Email, string Phone)? Mediums)>
            PhoneAndEmail) element)
    {
        return ValueTupleMarshaller.Marshall(element);
    }
}