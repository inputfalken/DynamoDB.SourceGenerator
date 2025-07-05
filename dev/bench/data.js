window.BENCHMARK_DATA = {
  "lastUpdate": 1751759505253,
  "repoUrl": "https://github.com/inputfalken/DynamoDB.SourceGenerator",
  "entries": {
    "Benchmark.Net Benchmark": [
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
          "id": "048ec8dd943b9926d6f12ed7c4448492aaafff86",
          "message": "Rename and filter out relevant benchmark",
          "timestamp": "2025-07-06T01:12:41+02:00",
          "tree_id": "8413de728cf2292695fd8390ab2371b0b2546e43",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/048ec8dd943b9926d6f12ed7c4448492aaafff86"
        },
        "date": 1751757397742,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_TimeOnly",
            "value": 218.2032169011923,
            "unit": "ns",
            "range": "± 0.2272505200988721"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_TimeOnly",
            "value": 88.75754918370929,
            "unit": "ns",
            "range": "± 0.26100517476373597"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateOnly",
            "value": 171.00989411558425,
            "unit": "ns",
            "range": "± 0.18666548207921393"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateOnly",
            "value": 94.34705809752147,
            "unit": "ns",
            "range": "± 0.6144401599079438"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTimeOffset",
            "value": 287.71047936167037,
            "unit": "ns",
            "range": "± 0.5193068487583273"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTimeOffset",
            "value": 112.17252842869077,
            "unit": "ns",
            "range": "± 0.6696003216513122"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTime",
            "value": 220.09310304323833,
            "unit": "ns",
            "range": "± 0.16266826689180122"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTime",
            "value": 111.64623548767783,
            "unit": "ns",
            "range": "± 2.741981165888981"
          }
        ]
      }
    ],
    "Temporal Type benchmark": [
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
          "id": "8fa308f36eed632b0de1f25dbe9cc0812ced475a",
          "message": "Fix",
          "timestamp": "2025-07-06T01:38:53+02:00",
          "tree_id": "7dd32ef8c12832f90ae8cdf8e15d6e94da30ecc5",
          "url": "https://github.com/inputfalken/DynamoDB.SourceGenerator/commit/8fa308f36eed632b0de1f25dbe9cc0812ced475a"
        },
        "date": 1751759504414,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_TimeOnly",
            "value": 214.12442796046918,
            "unit": "ns",
            "range": "± 0.07846611991406302"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_TimeOnly",
            "value": 97.55084981123606,
            "unit": "ns",
            "range": "± 0.38326942783522483"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateOnly",
            "value": 168.69175476687295,
            "unit": "ns",
            "range": "± 0.386935737688513"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateOnly",
            "value": 101.2095209757487,
            "unit": "ns",
            "range": "± 0.6295787339587091"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTimeOffset",
            "value": 286.7838045633756,
            "unit": "ns",
            "range": "± 0.29838423886343224"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTimeOffset",
            "value": 125.04675747553507,
            "unit": "ns",
            "range": "± 1.0111212129500844"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Unmarshall_DateTime",
            "value": 248.85044542948404,
            "unit": "ns",
            "range": "± 1.026340590536599"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.TemporalBenchmarks.Marshall_DateTime",
            "value": 119.3100028594335,
            "unit": "ns",
            "range": "± 0.9335212232601835"
          }
        ]
      }
    ]
  }
}