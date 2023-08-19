# DynamoDB.SourceGenerator

A .NET source generator whose goal is to automate the low-level DynamoDB API. 
While also being AOT compatible by not using any reflection in the source generated code.

## Note

This project has not been tested in any real scenario and currently serves as a hobby project.

## Features

### Type Marshalling

#### Primitive Types

| Type            | Field       |
| ---             | ---         |
| `bool`          | `BOOL`      |
| `char`          | `S`         |
| `int`           | `N`         |
| `long`          | `N`         |
| `string`        | `S`         |
| `uint`          | `N`         |
| `ulong`         | `N`         |

#### Temporal Types

| Type            | Field       | Format    |
| ---             | ---         | ---       |
| `DateOnly`      | `S`         | ISO 8601  |
| `DateTime`      | `S`         | ISO 8601  |
| `DateTimeOffset`| `S`         | ISO 8601  |

#### Collection Types

| Type                                  | Field       | Description                                           |
| ---                                   | ---         |-------------------------------------------------------|
| `ICollection<T>`                      | `L`         |                                                       |
| `IDictonary<string, TValue>`          | `M`         | Will treat the `Dictionary` as a **Key-Value** store. |
| `IEnumerable<T>`                      | `L`         |                                                       |
| `IReadOnlyList<T>`                    | `L`         |                                                       |
| `IRedonlyDictionary<string, TValue>`  | `M`         | Will treat the `Dictionary` as a **Key-Value** store. |
| `ISet<int>`                           | `NS`        |                                                       |
| `ISet<long>`                          | `NS`        |                                                       |
| `ISet<string>`                        | `SS`        |                                                       |
| `ISet<uint>`                          | `NS`        |                                                       |
| `ISet<ulong>`                         | `NS`        |                                                       |
| `T[]`                                 | `L`         |                                                       |

#### Custom types
For types not listed such as a custom entity class the source generator will treat as an object.
See below for more information.

```csharp
public class Person 
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Address 
{
    public string Id { get; set; }
    public string Street { get; set; }
}
```

| Type      | Field       | Description                      |
|-----------| ---         |----------------------------------|
| `Person`  | `M`         | All datamember will be included. |
| `Address` | `M`         | All datamember will be included. |

### Type Unmarshalling

TODO

### Reference Tracker

TODO

### Nullable reference types
The source generated code will adapt to your NullableReference types if you have it enabled.

##### Examples
```csharp
#nullable enable
// The following would be considered to be optional.
public string? MyOptionalString { get; set; }
// The following would be considered required and throw ArgumentNullException if the value was not provided.
public string MyRequiredString { get; set; }
#nullable disable
// The following does not have nullable enabled and would consider the string to be optional.
public string MyUnknownString { get; set; }
```

### Usage sample

```csharp
internal static class Program
{
    public static void Main()
    {
        var repository = new();
        var putItemRequest = repository.PersonDocument.ToPutItemRequest(new Person());
        ...
    }
}

public class Person
{
    public string Firstname { get; set; }
    public Contact ContactInfo { get; set; }

    public class Contact
    {
        public string Email { get; set;}
    }
}
// This DynamoDBDocumentAttribute is what will casuse the source generation to kick in.
// The type provided to the DynamoDBDocumentAttribute is what will get functinality. 
// It is possible to provide multiple DynamoDBDocumentAttributes in order to have multiple types source generated.
[DynamoDBDocument(typeof(Person))]
public partial class Repository { }
```

## Installing

There's no NuGet package available yet, In order to try it right now would be to clone this repository and play with the `SampleApp` assembly.

## Project structure

The `DynamoDBGenerator.SourceGenerator` assembly is responsible for doing the heavy lifting by generating the building blocks for the `DynamoDBGenerator` assembly to extend with convenient functionality.
Such as the [ToPutItemRequest](https://github.com/inputfalken/DynamoDB.SourceGenerator/blob/main/DynamoDBGenerator/Extensions/IDynamoDbDocumentExtensions.cs#L10) extension method.
