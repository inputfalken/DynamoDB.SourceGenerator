window.BENCHMARK_DATA = {
  "lastUpdate": 1751795993317,
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
      },
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
          "id": "a9034f9b8d34c15f8bb1ae93f53df7d37c1ecad0",
          "message": "Remove Artifacts",
          "timestamp": "2025-07-06T11:57:56+02:00",
          "tree_id": "ef755862ff9c01d1aa18c8e9b1bf9952de49dbee",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/a9034f9b8d34c15f8bb1ae93f53df7d37c1ecad0"
        },
        "date": 1751795992858,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Unmarshall_Person_DTO",
            "value": 1659.9186900002617,
            "unit": "ns",
            "range": "± 6.309732445183379"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Unmarshall_Person_DTO",
            "value": 13619.224263509115,
            "unit": "ns",
            "range": "± 78.2710707221475"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Marshall_Person_DTO",
            "value": 1428.9868203571864,
            "unit": "ns",
            "range": "± 8.264891274328109"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Marshall_Person_DTO",
            "value": 11385.227649943034,
            "unit": "ns",
            "range": "± 125.82659933408499"
          }
        ]
      }
    ]
  }
}