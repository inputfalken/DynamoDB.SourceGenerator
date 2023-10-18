# DynamoDB.SourceGenerator

This source generator is crafted to simplify DynamoDB integration for your projects. It's designed to effortlessly generate the low-level DynamoDB API tailored to any DTO you provide.

## Note
This project has not been tested in any real scenario and currently serves as a hobby project.

## Installation
Install  the following packages from Nuget:

[![DynamoDBGenerator][1]][2] 

[![DynamoDBGenerator.SourceGenerator][3]][4]

[1]: https://img.shields.io/nuget/v/DynamoDBGenerator.svg?label=DynamoDBGenerator
[2]: https://www.nuget.org/packages/DynamoDBGenerator
[3]: https://img.shields.io/nuget/v/DynamoDBGenerator.SourceGenerator.svg?label=DynamoDBGenerator.SourceGenerator
[4]: https://www.nuget.org/packages/DynamoDBGenerator.SourceGenerator



## Goals:

* Seamless Developer Interaction (DevEx): Experience a hassle-free DynamoDB interaction where the generated code handles
  the heavy lifting, ensuring an intuitive and convenient experience for developers.
  * Simplify Attribute Expressions: Easily create complex expressions with an intuitive approach.
* Faster performance: Utilize the low-level API that would normally be implemented manually.


## Features:

* Reflection-Free Codebase: The generated code is built without reliance on reflection, ensuring compatibility with
  Ahead-of-Time ([AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Cwindows)) compilation: This translates to faster startup times and a more efficient memory footprint.
* Nullable Reference type support: Embrace modern coding practices with support for [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references).
  Effortlessly handle optional values and ensure robust error handling.
* Marshalling: Seamlessly convert your DTO into DynamoDB types.
* Unmarshalling: Seamlessly convert DynamoDB types into your DTO.
  * Constructor support: Leverage constructors in your DTOs.
* Marshalling keys: Seamlessly convert values into DynamoDB key types by decorating your properties with DynamoDbKeysAttributes.
  * HashKey and RangeKey ✔ 
  * GlobalSecondaryIndex ✔
  * LocalSecondaryIndex ✔
       

## TODO
* Streamlined DynamoDBClient based on the source-generated code.
* Add methods to convert to request objects.

## Conversion

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

### Binary

| Type           | Field |
|----------------|-------|
| `MemoryStream` | `B`   |


### Temporal Types

| Type             | Field | Format   |
|------------------|-------|----------|
| `DateOnly`       | `S`   | ISO 8601 |
| `DateTime`       | `S`   | ISO 8601 |
| `DateTimeOffset` | `S`   | ISO 8601 |

### Collection Types

| Type                                 | Field | Description                                           |
|--------------------------------------|-------|-------------------------------------------------------|
| `ICollection<T>`                     | `L`   |                                                       |
| `IDictonary<string, TValue>`         | `M`   | Will treat the `Dictionary` as a **Key-Value** store. |
| `IEnumerable<T>`                     | `L`   |                                                       |
| `IReadOnlyList<T>`                   | `L`   |                                                       |
| `IRedonlyDictionary<string, TValue>` | `M`   | Will treat the `Dictionary` as a **Key-Value** store. |
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

For an illustrative example, refer to the [tests](https://github.com/inputfalken/DynamoDB.SourceGenerator/blob/main/tests/DynamoDBGenerator.SourceGenerator.Tests/Extensions/ToAttributeExpressionTests.cs).

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

## Usage sample

```csharp
internal static class Program
{
    public static void Main()
    {
        Repository repository = new Repository();
        PutItemRequest putItemRequest = repository.PersonMarshaller.ToPutItemRequest(new Person(), "TABLENAME");
    }
}

public class Person
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
        public string Email { get; set;}
    }
}

// This DynamoDBMarshallerAttribute is what will cause the source generation to kick in.
// The type provided to the DynamoDBMarshallerAttribute is what will get functionality.
// It is possible to provide multiple DynamoDBMarshallerAttributes in order to source-generate multiple types in the same class.
[DynamoDBMarshaller(typeof(Person), PropertyName = "PersonMarshaller")]
public partial class Repository { }
```

## Project structure

The `DynamoDBGenerator.SourceGenerator` assembly is responsible for doing the heavy lifting by generating the building
blocks for the `DynamoDBGenerator` assembly to extend with convenient functionality.
Such as
the [ToPutItemRequest](https://github.com/inputfalken/DynamoDB.SourceGenerator/blob/main/tests/DynamoDBGenerator.SourceGenerator.Tests/Extensions/ToPutItemRequestTests.cs)
extension method.
