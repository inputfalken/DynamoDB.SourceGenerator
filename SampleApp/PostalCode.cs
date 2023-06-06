using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBGenerator;

namespace SampleApp;

public partial class PostalCode
{
    public string ZipCode { get; set; } = null!;

    public string Town { get; set; } = null!;
    public KeyValuePair<string, int> String { get; set; }

    // We could provide a cache to determine whether this symbol has already been generated amd filter it out to avoid recursive constructor.
    public Address Address { get; set; }

}