window.BENCHMARK_DATA = {
  "lastUpdate": 1751795897263,
  "repoUrl": "https://github.com/inputfalken/DynamoDB.SourceGenerator",
  "entries": {
    "Comparison Marshalling": [
      {
        "commit": {
          "author": {
            "email": "inputfalken@gmail.com",
            "name": "Robert Andersson",
            "username": "inputfalken"
          },
          "committer": {
            "email": "inputfalken@gmail.com",
            "name": "Robert Andersson",
            "username": "inputfalken"
          },
          "distinct": true,
          "id": "3977afc7f246a352127b30ece11f09b06c581c61",
          "message": "Change order of benchmarks",
          "timestamp": "2025-07-06T11:56:25+02:00",
          "tree_id": "07b8d2226d320803f32c25e16113218adac49cd8",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/3977afc7f246a352127b30ece11f09b06c581c61"
        },
        "date": 1751795896788,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Unmarshall_Person_DTO",
            "value": 1635.3654059001378,
            "unit": "ns",
            "range": "± 5.033207216902051"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Unmarshall_Person_DTO",
            "value": 13347.22516305106,
            "unit": "ns",
            "range": "± 67.37644764810707"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Marshall_Person_DTO",
            "value": 1479.8897932688394,
            "unit": "ns",
            "range": "± 13.418593179740876"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Marshall_Person_DTO",
            "value": 11271.735316975912,
            "unit": "ns",
            "range": "± 85.24968488990449"
          }
        ]
      }
    ]
  }
}