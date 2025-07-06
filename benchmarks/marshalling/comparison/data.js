window.BENCHMARK_DATA = {
  "lastUpdate": 1751795633921,
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
          "id": "94f92575e66268087fb0c8eef711e391b4cd6348",
          "message": "Fix comparison benchmark",
          "timestamp": "2025-07-06T11:51:54+02:00",
          "tree_id": "d40a90357e34c737f95b683618204fcca8f1eda6",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/94f92575e66268087fb0c8eef711e391b4cd6348"
        },
        "date": 1751795633563,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Unmarshall_Person_DTO",
            "value": 1639.9147553077112,
            "unit": "ns",
            "range": "± 4.353175276469287"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Marshall_Person_DTO",
            "value": 1432.8698444366455,
            "unit": "ns",
            "range": "± 18.68606283195657"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Unmarshall_Person_DTO",
            "value": 13768.318939208984,
            "unit": "ns",
            "range": "± 149.9942097909306"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.ComparisonBenchmarks.Amazon_Marshall_Person_DTO",
            "value": 11377.686829630535,
            "unit": "ns",
            "range": "± 92.539347858278"
          }
        ]
      }
    ]
  }
}