window.BENCHMARK_DATA = {
  "lastUpdate": 1751759508430,
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
    ],
    "Primitive Type benchmark": [
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
        "date": 1751759506164,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Bool",
            "value": 22.2761067705495,
            "unit": "ns",
            "range": "± 0.04249935478732524"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Bool",
            "value": 61.88611660003662,
            "unit": "ns",
            "range": "± 0.5188937937737125"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Char",
            "value": 26.94814132054647,
            "unit": "ns",
            "range": "± 0.07459585650837124"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Char",
            "value": 82.72960697015127,
            "unit": "ns",
            "range": "± 0.6360093642230397"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Int32",
            "value": 36.41015049318472,
            "unit": "ns",
            "range": "± 0.033018185375013484"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Int",
            "value": 80.27588557402292,
            "unit": "ns",
            "range": "± 0.514526791906238"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Int64",
            "value": 31.846653846594002,
            "unit": "ns",
            "range": "± 0.04469476313436696"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Int64",
            "value": 79.48245931863785,
            "unit": "ns",
            "range": "± 1.126049332988225"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_String",
            "value": 23.058590996265412,
            "unit": "ns",
            "range": "± 0.06865600054800972"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_String",
            "value": 79.03075147072474,
            "unit": "ns",
            "range": "± 0.42143366449022646"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_UInt32",
            "value": 36.38432812690735,
            "unit": "ns",
            "range": "± 0.054362542168663444"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_UInt32",
            "value": 79.7289938489596,
            "unit": "ns",
            "range": "± 0.4653436836646742"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_UInt64",
            "value": 32.43046822292464,
            "unit": "ns",
            "range": "± 0.031214881924812577"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Uint64",
            "value": 81.42023704051971,
            "unit": "ns",
            "range": "± 1.09748445702562"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Guid",
            "value": 67.01725242688106,
            "unit": "ns",
            "range": "± 0.07405228122082905"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Guid",
            "value": 92.66441837151845,
            "unit": "ns",
            "range": "± 0.6241351528319973"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Enum",
            "value": 29.01228967308998,
            "unit": "ns",
            "range": "± 0.0602483834187857"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Enum",
            "value": 78.14252279599508,
            "unit": "ns",
            "range": "± 0.428847197742131"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Unmarshall_Double",
            "value": 65.94297291682317,
            "unit": "ns",
            "range": "± 0.0392412139360464"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.PrimitiveBenchmarks.Marshall_Double",
            "value": 175.69382265897897,
            "unit": "ns",
            "range": "± 0.7819055043302933"
          }
        ]
      }
    ],
    "Collection Type benchmark": [
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
        "date": 1751759507779,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Unmarshall_Dictionary",
            "value": 264.2741312980652,
            "unit": "ns",
            "range": "± 3.768450242084289"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Marshall_Dictionary",
            "value": 343.961410352162,
            "unit": "ns",
            "range": "± 2.6907764187501564"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Unmarshall_StringHashSet",
            "value": 105.04170739650726,
            "unit": "ns",
            "range": "± 0.4765021851265234"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Marshall_StringHashSet",
            "value": 133.382590341568,
            "unit": "ns",
            "range": "± 1.5715673771069143"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Unmarshall_StringList",
            "value": 258.6355338830214,
            "unit": "ns",
            "range": "± 0.222937657369255"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Marshall_StringList",
            "value": 360.752862739563,
            "unit": "ns",
            "range": "± 2.5093837181376473"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Unmarshall_IntHashSet",
            "value": 97.11636657714844,
            "unit": "ns",
            "range": "± 0.5128803111477501"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Marshall_IntHashSet",
            "value": 132.0107527732849,
            "unit": "ns",
            "range": "± 0.9004847346988197"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Unmarshall_KeyValuePairList",
            "value": 364.32894903818766,
            "unit": "ns",
            "range": "± 3.516281983048862"
          },
          {
            "name": "DynamoDBGenerator.SourceGenerator.Benchmarks.Benchmarks.Marshalling.CollectionBenchmarks.Marshall_KeyValuePairList",
            "value": 694.2521739323934,
            "unit": "ns",
            "range": "± 6.6858826676921375"
          }
        ]
      }
    ]
  }
}