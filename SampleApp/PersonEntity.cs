using System;
using System.Collections.Generic;
using DynamoDBGenerator;

namespace SampleApp;

[AttributeValueGenerator]
public partial class PersonEntity
{
    public string Id { get; set; }
    public Address Address { get; set; }

}