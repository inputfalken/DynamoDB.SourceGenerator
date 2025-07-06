window.BENCHMARK_DATA = {
  "lastUpdate": 1751792769290,
  "repoUrl": "https://github.com/inputfalken/DynamoDB.SourceGenerator",
  "entries": {
    "Temporal Marshalling": [
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
          "id": "51508ea0d44d32efff975ac1720c71df81c479e4",
          "message": "Fix name",
          "timestamp": "2025-07-06T11:02:10+02:00",
          "tree_id": "12816b63c932ef3844c6d14dd2caa5c5c596b4cc",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/51508ea0d44d32efff975ac1720c71df81c479e4"
        },
        "date": 1751792768422,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_TimeOnly",
            "value": 207.4078583258849,
            "unit": "ns",
            "range": "± 0.4218326926054652"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_TimeOnly",
            "value": 88.17657451232274,
            "unit": "ns",
            "range": "± 1.5539937551972607"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateOnly",
            "value": 167.24196486813682,
            "unit": "ns",
            "range": "± 0.2008773245219396"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateOnly",
            "value": 92.12333978925433,
            "unit": "ns",
            "range": "± 0.5951774434178916"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTimeOffset",
            "value": 281.898257622352,
            "unit": "ns",
            "range": "± 0.23934231781758136"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTimeOffset",
            "value": 114.80914856378848,
            "unit": "ns",
            "range": "± 0.8924973450041866"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTime",
            "value": 217.49416259618906,
            "unit": "ns",
            "range": "± 0.3156324475147781"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTime",
            "value": 110.6788691745864,
            "unit": "ns",
            "range": "± 1.982125400660093"
          }
        ]
      }
    ]
  }
}