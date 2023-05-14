# DynamoDB.SourceGenerator

A .NET source generator whose goal is to automate the low-level DynamoDB API.

## Note

This project has not been tested in any real scenario and currently serves as mostly a hobby project.

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

#### Object types
TODO...
