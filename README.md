# DynamoDB.SourceGenerator

A .NET source generator whose goal is to automate the low-level DynamoDB API.

## Note

This project has not been tested in any real scenario and currently serves as a hobby project.

## Features

### POCO to AttributeValue Conversion

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

| Type                                  | Field       | Description                                                                                                   |
| ---                                   | ---         | ---                                                                                                           |
| `ICollection<T>`                      | `L`         |                                                                                                               |
| `IDictonary<string, TValue>`          | `M`         | Will treat the `Dictionary` as a **Key-Value** store and use the key of the `Dictionary` as the object `Key`. |
| `IEnumerable<T>`                      | `L`         |                                                                                                               |
| `IReadOnlyList<T>`                    | `L`         |                                                                                                               |
| `IRedonlyDictionary<string, TValue>`  | `M`         | Will treat the `Dictionary` as a **Key-Value** store and use the key of the `dictionary` as the object `Key`. |
| `ISet<int>`                           | `NS`        |                                                                                                               |
| `ISet<long>`                          | `NS`        |                                                                                                               |
| `ISet<string>`                        | `SS`        |                                                                                                               |
| `ISet<uint>`                          | `NS`        |                                                                                                               |
| `ISet<ulong>`                         | `NS`        |                                                                                                               |
| `T[]`                                 | `L`         |                                                                                                               |

#### Nullable reference types
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

#### Object types
TODO...

### Usage

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
[DynamoDBDocument(typeof(Person))]
public class Repository { }
```
