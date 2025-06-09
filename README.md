# DynamoDB.SourceGenerator

This source generator is crafted to simplify DynamoDB integration for your projects. It's designed to effortlessly
generate the low-level DynamoDB API tailored to any DTO you provide.

## Installation

If you want access to more high level reusable abstractions, utilizing builder patterns from the functionality of this
library, check out [Dynatello!](https://github.com/inputfalken/Dynatello)

---

Install the following dependencies:

[![DynamoDBGenerator][1]][2]

[![DynamoDBGenerator.SourceGenerator][3]][4]

[1]: https://img.shields.io/nuget/v/DynamoDBGenerator.svg?label=DynamoDBGenerator

[2]: https://www.nuget.org/packages/DynamoDBGenerator

[3]: https://img.shields.io/nuget/v/DynamoDBGenerator.SourceGenerator.svg?label=DynamoDBGenerator.SourceGenerator

[4]: https://www.nuget.org/packages/DynamoDBGenerator.SourceGenerator

The `DynamoDBGenerator.SourceGenerator` is where the source generator is implemented.
The source generator will look for attributes and implement interfaces that exists in `DynamoDBGenerator`.

## Goals:

* Seamless Developer Interaction (DevEx): Experience a hassle-free DynamoDB interaction where the generated code handles
  the heavy lifting, ensuring an intuitive and convenient experience for developers.
    * Simplify Attribute Expressions: Easily create complex expressions with an intuitive approach.
* Faster performance: Utilize the low-level API that would normally be implemented manually.

## [Benchmarks](tests/DynamoDBGenerator.SourceGenerator.Benchmarks): 

Here's a quick summary about how this library performs with a quick example of marshalling and unmarshalling a simple DTO object.

| Method                      | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Marshall_AWS_Reflection     | 2,467.8 ns | 48.87 ns | 68.51 ns | 0.5112 | 0.0038 |    6450 B |
| Marshall_Source_Generated   |   234.0 ns |  2.92 ns |  2.73 ns | 0.1421 | 0.0010 |    1784 B |
| Unmarshall_AWS_Reflection   | 2,397.0 ns | 36.13 ns | 30.17 ns | 0.5188 | 0.0038 |    6544 B |
| Unmarshall_Source_Generated |   131.6 ns |  0.82 ns |  0.77 ns | 0.0126 |      - |     160 B |

## Features:

* Reflection-Free Codebase: The generated code is built without reliance on reflection, ensuring compatibility with
  Ahead-of-Time ([AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Cwindows))
  compilation: This translates to faster startup times and a more efficient memory footprint.
* Nullable Reference type support: Embrace modern coding practices with support
  for [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references).
  Effortlessly handle optional values and ensure robust error handling.
* Marshalling: Seamlessly convert your DTO into DynamoDB types.
* Unmarshalling: Seamlessly convert DynamoDB types into your DTO.
    * Constructor support: Leverage constructors in your DTOs.
* Marshalling keys: Seamlessly convert values into DynamoDB key types by decorating your properties with
  DynamoDbKeysAttributes.
    * HashKey and RangeKey ✔
    * GlobalSecondaryIndex ✔
    * LocalSecondaryIndex ✔
* Custom Converters: Create converters for your own types or override
  the [default converters](https://github.com/inputfalken/DynamoDB.SourceGenerator/blob/main/src/DynamoDBGenerator/Options/AttributeValueConverters.cs)
  built in to the library.
* `ValueTuple<T>` support: You don't have to declare your own types and could instead
  use [tuples](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples) with
  custom named fields that will act as if the tuple was a type with those data members.
* Improved performance  see [benchmark](./tests/DynamoDBGenerator.SourceGenerator.Benchmarks) project.

## Default conversion

If you do not override the conversion behaviour the following rules will be applied

### Primitive Types

| Type     | Field  |
|----------|--------|
| `bool`   | `BOOL` |
| `char`   | `S`    |
| `int`    | `N`    |
| `long`   | `N`    |
| `string` | `S`    |
| `uint`   | `N`    |
| `ulong`  | `N`    |
| `Guid`   | `S`    |
| `Enum`   | `N`    |

### Binary

| Type           | Field |
|----------------|-------|
| `MemoryStream` | `B`   |

### Temporal Types

| Type             | Field | Format   |
|------------------|-------|----------|
| `DateOnly`       | `S`   | ISO 8601 |
| `TimeOnly`       | `S`   | ISO 8601 |
| `DateTime`       | `S`   | ISO 8601 |
| `DateTimeOffset` | `S`   | ISO 8601 |
| `TimeSpan`       | `S`   | ISO 8601 |

### Collection Types

| Type                                 | Field | Description                                           |
|--------------------------------------|-------|-------------------------------------------------------|
| `ICollection<T>`                     | `L`   |                                                       |
| `IDictonary<string, TValue>`         | `M`   | Will treat the `Dictionary` as a **Key-Value** store. |
| `IEnumerable<T>`                     | `L`   |                                                       |
| `IReadOnlyList<T>`                   | `L`   |                                                       |
| `IReadonlyDictionary<string, TValue>` | `M`   | Will treat the `Dictionary` as a **Key-Value** store. |
| `ILookup<string, TValue>`            | `M`   | Will treat the `ILookup` as a **Key-Values** store.   |
| `ISet<int>`                          | `NS`  |                                                       |
| `ISet<long>`                         | `NS`  |                                                       |
| `ISet<string>`                       | `SS`  |                                                       |
| `ISet<uint>`                         | `NS`  |                                                       |
| `ISet<ulong>`                        | `NS`  |                                                       |
| `T[]`                                | `L`   |                                                       |

### Custom types

Types not listed above will be treated as an object by being assigned to the `M` field

## Reference Tracker

As part of the source generation process, two additional types will be mirrored to the provided DTO:

* A reference tracker that serves as attribute references on DynamoDB side.
* A reference tracker that functions as attribute references for the arguments you provide to DynamoDB.

These trackers enable you to consistently construct your AttributeExpressions using string interpolation.
For an illustrative example, refer to
the [tests](tests/DynamoDBGenerator.SourceGenerator.Tests/Extensions/ToAttributeExpressionTests.cs).

## Nullable reference types

The source-generated code will adapt to your NullableReference types if you have it enabled.

### Examples

```csharp
#nullable enable
// The following would be considered to be optional.
public string? MyOptionalString { get; set; }
// The following would be considered required and throw DynamoDBMarshallingException if the value was not provided.
public string MyRequiredString { get; set; }
#nullable disable
// The following does not have nullable enabled and would consider the string to be optional.
public string MyUnknownString { get; set; }
```

## Code examples

### [Initializers](./samples/Initialisation/Program.cs)

You can instruct the marshaller to use a constructor by applying the `[DynamoDBMarshallerConstructor]` attribute ontop
of the desired constructor.
Or use the object Initializer syntax.

```csharp
_ = new Constructor(id: "123", count: 10);
_ = new Initializer { Id = "123", Count = 10 };

[DynamoDBMarshaller]
public partial class Constructor
{
    [DynamoDBMarshallerConstructor]
    public Constructor(string id, int count)
    {
        Count = count;
        Id = id;
    }

    public int Count { get; }
    public string Id { get; }
}

[DynamoDBMarshaller]
public partial class Initializer
{
    public string Id { get; set; }
    public int Count { get; set; }
}
```

### [Type support](./samples/TypeSupport)

The functionality can be applied to more than classes:

```csharp
[DynamoDBMarshaller]
public partial record Record([property: DynamoDBHashKey] string Id);

[DynamoDBMarshaller]
public partial class Class
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public partial struct Struct
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public readonly partial struct ReadOnlyStruct
{
    [DynamoDBHashKey]
    public string Id { get; init; }
}

[DynamoDBMarshaller]
public readonly partial record struct ReadOnlyRecordStruct([property: DynamoDBHashKey] string Id);
```

### [DTO sample](./samples/RequestAndResponseObjects/Person.cs)

An example DTO class could look like the one below.

**The following request examples will reuse this DTO.**

```csharp
// A typical scenario would be that you would use multiple DynamoDBMarshaller and describe your operations via AccessName.
// If you do not specify an ArgumentType it will use your main entity Type instead which is typically useful for PUT operations.
[DynamoDBMarshaller]
[DynamoDBMarshaller(ArgumentType = typeof((string PersonId, string Firstname)), AccessName = "UpdateFirstName")]
[DynamoDBMarshaller(ArgumentType = typeof(string), AccessName = "GetById")]
public partial class Person
{
    // Will be included as 'PK' in DynamoDB.
    [DynamoDBHashKey("PK")]
    public string Id { get; set; }

    // Will be included as 'Firstname' in DynamoDB.
    public string Firstname { get; set; }

    // Will be included as 'Contact' in DynamoDB.
    [DynamoDBProperty("Contact")]
    public Contact ContactInfo { get; set; }

    // Wont be included in DynamoDB.
    [DynamoDBIgnore]
    public string FirstNameLowercase => Firstname.ToLower();

    public class Contact
    {
        // Will be included as 'Email' in DynamoDB.
        public string Email { get; set; }
    }
}
```

### Creating request objects

#### [Put request](./samples/RequestAndResponseObjects/Program.cs)

```csharp
static PutItemRequest PutPerson()
{
    return new PutItemRequest
    {
        TableName = "MyTable",
        Item = Person.PersonMarshaller.Marshall(new Person
        {
            Firstname = "John",
            Id = Guid.NewGuid().ToString(),
            ContactInfo = new Person.Contact { Email = "john@test.com" }
        })
    };
}
```

#### [Get Request & Response](./samples/RequestAndResponseObjects/Program.cs)

```csharp

static GetItemRequest CreateGetItemRequest()
{
    return new GetItemRequest
    {
        Key = Person.GetById.PrimaryKeyMarshaller.PartitionKey("123"),
        TableName = "MyTable"
    };
}

static Person DeserializeResponse(GetItemResponse response)
{
    if (response.HttpStatusCode != HttpStatusCode.OK)
        throw new NotImplementedException();
    
    return Person.GetById.Unmarshall(response.Item);
}

```

#### [Update request without providing the DTO](./samples/RequestAndResponseObjects/Program.cs)

```csharp
static UpdateItemRequest UpdateFirstName()
{
    // Creating an AttributeExpression can be done through string interpolation where the source generator will mimic your DTO types and give you an consistent API to build the attributeExpressions.
    var attributeExpression = Person.UpdateFirstName.ToAttributeExpression(
        ("personId", "John"),
        (dbRef, argRef) => $"{dbRef.Id} = {argRef.PersonId}", // The condition
        (dbRef, argRef) => $"SET {dbRef.Firstname} = {argRef.Firstname}" // The update operation
    );

    // the index can be used to retrieve the expressions in the same order as you provide the string interpolations in the method call above.
    var condition = attributeExpression.Expressions[0];
    var update = attributeExpression.Expressions[1];
    var keys = Person.UpdateFirstName.PrimaryKeyMarshaller.PartitionKey("personId");

    return new UpdateItemRequest
    {
        ConditionExpression = condition,
        UpdateExpression = update,
        ExpressionAttributeNames = attributeExpression.Names,
        ExpressionAttributeValues = attributeExpression.Values,
        Key = keys,
        TableName = "MyTable"
    };
}
```

### [Key conversion](./samples/KeyConversion/Program.cs)

The key marshallers contain three methods based on your intent.
The source generator will internally validate your object arguments. So if you pass a `int` but the actual key is
represented as a `string`, then you will get an `exception`.

* `Keys(object partitionKey, object rangeKey)`
    * Used when you want convert both a partition key and a range key.
* `PartionKey(object key)`
    * Used when you only want to only convert a partition key without a range key.
* `RangeKey(object key)`
    * Used when you only want to only convert a range key without a partition key.

```csharp
// PrimaryKeyMarshaller is used to convert the keys obtained from the [DynamoDBHashKey] and [DynamoDBRangeKey] attributes.
var keyMarshaller = EntityDTO.KeyMarshallerSample.PrimaryKeyMarshaller;

// IndexKeyMarshaller requires an argument that is the index name so it can provide you with the correct conversion based on the indexes you may have.
// It works the same way for both LocalSecondaryIndex and GlobalSecondaryIndex attributes.
var GSIKeyMarshaller = EntityDTO.KeyMarshallerSample.IndexKeyMarshaller("GSI");
var LSIKeyMarshaller = EntityDTO.KeyMarshallerSample.IndexKeyMarshaller("LSI");

[DynamoDBMarshaller(AccessName = "KeyMarshallerSample")]
public partial class EntityDTO
{
    [DynamoDBHashKey("PK")]
    public string Id { get; set; }

    [DynamoDBRangeKey("RK")]
    public string RangeKey { get; set; }

    [DynamoDBLocalSecondaryIndexRangeKey("LSI")]
    public string SecondaryRangeKey { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("GSI")]
    public string GlobalSecondaryIndexId { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI")]
    public string GlobalSecondaryIndexRangeKey { get; set; }
}
```

### Configuring marshalling behaviour

By applying the DynamoDbMarshallerOptions you're able to configure all DynamoDBMarshallers that's declared on the same
type.

#### [Custom converters](./samples/Configuration/Program.cs)

```csharp
[DynamoDbMarshallerOptions(Converters = typeof(MyCustomConverters))]
[DynamoDBMarshaller]
public partial record OverriddenConverter([property: DynamoDBHashKey] string Id, DateTime Timestamp);

// Implement a converter, there's also an IReferenceTypeConverter available for ReferenceTypes.
public class UnixEpochDateTimeConverter : IValueTypeConverter<DateTime>
{
    public UnixEpochDateTimeConverter()
    {
    }

    // Convert the AttributeValue into a .NET type.
    public DateTime? Read(AttributeValue attributeValue)
    {
        return long.TryParse(attributeValue.N, out var epoch)
            ? DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime
            : null;
    }

    // Convert the .NET type into an AttributeValue.
    public AttributeValue Write(DateTime element)
    {
        return new AttributeValue { N = new DateTimeOffset(element).ToUnixTimeSeconds().ToString() };
    }
}

// Create a new Converters class
// You don't have to inherit from AttributeValueConverters if you do not want to use the default converters provided.
public class MyCustomConverters : AttributeValueConverters
{
    // If you take constructor parameters, the source generator will recongnize it and change the way you access it into an method.
    // It's recommended to call the method once and save it into a class member.
    public MyCustomConverters()
    {
        // Override the default behaviour.
        DateTimeConverter = new UnixEpochDateTimeConverter();
    }
    // You could add more converter DataMembers as fields or properties to add your own custom conversions.
}
```

#### [Enum conversion](./samples/Configuration/EnumBehaviour.cs)

```csharp
[DynamoDbMarshallerOptions(EnumConversion = EnumConversion.Name)]
[DynamoDBMarshaller]
public partial record EnumBehaviour([property: DynamoDBHashKey] string Id, DayOfWeek Enum);
```

## Project structure

The `DynamoDBGenerator` assembly contains functionality that the `DynamoDBGenerator.SourceGenerator` rely on such as
the [attribute](src/DynamoDBGenerator/Attributes/DynamoDBMarshallerAttribute.cs) that will trigger the source
generation.
In other words both assemblies needs to be installed in order for the source generator to work as expected.
