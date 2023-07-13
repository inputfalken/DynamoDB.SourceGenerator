using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

public partial class PersonEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; }

    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public Address Address { get; set; }
    public (string, string, string) Test { get; set; }
    

}