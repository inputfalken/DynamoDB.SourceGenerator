window.BENCHMARK_DATA = {
  "lastUpdate": 1751791991189,
  "repoUrl": "https://github.com/inputfalken/DynamoDB.SourceGenerator",
  "entries": {
    "Temporal": [
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
          "id": "e72aa8facb6e64fedffabbf5e6ff97f80ca8d2b7",
          "message": "Split benchmark into different executions",
          "timestamp": "2025-07-06T10:49:06+02:00",
          "tree_id": "662196d99d5adda1ce660a979abadbce93f767b2",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/e72aa8facb6e64fedffabbf5e6ff97f80ca8d2b7"
        },
        "date": 1751791990877,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_TimeOnly",
            "value": 210.9593053322572,
            "unit": "ns",
            "range": "± 0.38818779792458136"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_TimeOnly",
            "value": 90.76045648540769,
            "unit": "ns",
            "range": "± 0.7915724035137934"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateOnly",
            "value": 170.51000792639596,
            "unit": "ns",
            "range": "± 0.20550749256750442"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateOnly",
            "value": 95.31157069206238,
            "unit": "ns",
            "range": "± 0.9714602163246566"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTimeOffset",
            "value": 280.5176492418562,
            "unit": "ns",
            "range": "± 0.37326569341115157"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTimeOffset",
            "value": 117.80560914207908,
            "unit": "ns",
            "range": "± 3.8735347019499287"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTime",
            "value": 224.31072771549225,
            "unit": "ns",
            "range": "± 1.4772737354431253"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTime",
            "value": 110.40632688999176,
            "unit": "ns",
            "range": "± 0.7367299521207623"
          }
        ]
      }
    ]
  }
}