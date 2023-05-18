using System;
using System.Collections.Generic;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class PersonEntity
{
    public IEnumerable<KeyValuePair<string, int>> Type { get; set; } = null!;

    public string MyHashKey { get; set; } = null!;

    public (int X, int Y, int Z) Coordinate { get; set; }
    public (int X, int Y, int Z) Coordinate2 { get; set; }
    public (int X, int Y, int Z) Coordinate4 { get; set; }
    public (int X, int Y, int Z) Coordinate5 { get; set; }
    public (int X, int Y, int Z) Coordinate6 { get; set; }
}