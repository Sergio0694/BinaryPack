![BinaryPackIcon](https://user-images.githubusercontent.com/10199417/67103112-d8852800-f1c4-11e9-9679-8cb344e988dc.png) [![NuGet](https://img.shields.io/nuget/v/BinaryPack.svg?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/BinaryPack/) [![NuGet](https://img.shields.io/nuget/dt/BinaryPack.svg?style=for-the-badge)](https://www.nuget.org/stats/packages/BinaryPack?groupby=Version)
[![AppVeyor](https://img.shields.io/appveyor/ci/Sergio0694/binarypack/master.svg?style=for-the-badge&logo=appveyor)](https://ci.appveyor.com/project/Sergio0694/binarypack/master/)

# What is it?

**BinaryPack** is a binary serialization library inspired by `MessagePack`, but faster and producing even smaller files. The goal of this project is to be able to use **BinaryPack** as a drop-in replacement for JSON/XML/MessagePack serialization, when the serialized models don't need to be shared with other applications or with web services. **BinaryPack** is built to be as fast and memory efficient as possible: it uses virtually no memory allocations, and the serialized data is packed to take up as little space as possible. Whenever you're using either JSON, MessagePack, XML or some other format to cache data for your apps, to send data between clients or to save data that is not critical, you can try using **BinaryPack** over your previous serialization library - it will provide the same basic functionalities to serialize and deserialize models, but with much higher performance and less memory usage.

### Alright, it's "faster and more efficient", but how much?
Of course you shouldn't take my word for it, so this README includes a number of benchmarks that were performed on a number of different models. The benchmark code and all the models used can be found in this repository. To summarize:
- **BinaryPack** was consistently the faster library, both during serialization and deserialization. The performance difference ranged from **7.6x** faster than `Newtonsoft.Json`, **7x** than [`Utf8Json`](https://github.com/neuecc/Utf8Json) and **1.9x** than `MessagePack` when serializing a small JSON response, to **245x** faster than `Newtonsoft.Json`, **129x** than `Utf8Json` and **3.9x** than `MessagePack` when dealing with mostly binary data (eg. a model with a large `float[]` array, or another `unmanaged` type).
- The memory usage was on average on par with `Utf8Json`, except when deserializing mostly binary data, in which case **BinaryPack** used **1/100** the memory of `Utf8Json`, and **1/2** that of `MessagePack`. **BinaryPack** also almost always resulted in the lowest number of GC collections during serialization and deserialization of models.
- In all cases, the **BinaryPack** serialization resulted in the smallest file on disk.

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [Quick start](#quick-start)
  - [Supported properties](#supported-properties)
- [Benchmarks](#benchmarks)
- [Requirements](#requirements)
- [Special thanks](#special-thanks)

# Installing from NuGet

To install **BinaryPack**, run the following command in the **Package Manager Console**

```
Install-Package BinaryPack
```

More details available [here](https://www.nuget.org/packages/BinaryPack/).

# Quick start

**BinaryPack** exposes a `BinaryConverter` class that acts as entry point for all public APIs. Every serialization API is available in an overload that works on a `Stream` instance, and one that instead uses the new `Memory<T>` APIs.

The following sample shows how to serialize and deserialize a simple model.

```C#
// Assume that this class is a simple model with a few properties
var model = new Model { Text = "Hello world!", Date = DateTime.Now, Values = new[] { 3, 77, 144, 256 } };

// Serialize to a memory buffer
var data = BinaryConverter.Serialize(model);

// Deserialize the model
var loaded = BinaryConverter.Deserialize<Model>(data);
```

## Supported properties

Here is a list of the property types currently supported by the library:

✅ Primitive types (except `object`): `string`, `bool`, `int`, `uint`, `float`, `double`, etc.

✅ Nullable value types: `Nullable<T>` or `T?` for short, where `T : struct`

✅ Unmanaged types: eg. `System.Numerics.Vector2`, and all `unmanaged` value types

✅ .NET collections: `List<T>`, `T[]`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, etc.

✅ .NET dictionaries: `Dictionary<TKey, TValue>` and `IDictionary<TKey, TValue>`

# Benchmarks
Here are three full benchmarks executed with the benchmark sample included in this repository. The error and standard deviation columns have been removed to fit each table in the horizontal space available for the README file reader on GitHub. The JSON response model used in the first two benchmarks is the [JsonResponseModel](https://github.com/Sergio0694/BinaryPack/blob/master/unit/BinaryPack.Models/JsonResponseModel.cs) class, using 240 child items in the first case, and 2000 in the second. The class used in the last benchmark is instead [NeuralNetworkLayerModel](https://github.com/Sergio0694/BinaryPack/blob/master/unit/BinaryPack.Models/NeuralNetworkLayerModel.cs).

### Small JSON response (~800KB)

|          Method |      Categories |       Mean | Ratio |     Gen 0 |    Gen 1 |    Gen 2 |  Allocated |
|---------------- |---------------- |-----------:|------:|----------:|---------:|---------:|-----------:|
|  NewtonsoftJson |   Serialization |   8.961 ms |  1.00 |  359.3750 | 234.3750 | 218.7500 |   616388 B |
| BinaryFormatter |   Serialization |   9.310 ms |  1.04 |  375.0000 | 171.8750 |  93.7500 |  1331250 B |
|     NetCoreJson |   Serialization |   9.753 ms |  1.09 |  437.5000 | 218.7500 | 203.1250 |  1006896 B |
|   XmlSerializer |   Serialization |  10.521 ms |  1.17 |  875.0000 | 171.8750 | 156.2500 |  3039637 B |
|        Utf8Json |   Serialization |   7.847 ms |  0.88 |  156.2500 | 156.2500 | 156.2500 |      243 B |
|     MessagePack |   Serialization |   2.141 ms |  0.24 |  222.6563 | 222.6563 | 222.6563 |     1187 B |
|      **BinaryPack** |   Serialization |   **1.124 ms** |  **0.13** |   **23.4375** |  **23.4375** |  **23.4375** |      **158 B** |
|                 |                 |            |       |           |          |          |            |
|  NewtonsoftJson | Deserialization |  13.828 ms |  1.00 |  484.3750 | 234.3750 |        - |  2866728 B |
| BinaryFormatter | Deserialization |  16.316 ms |  1.18 |  906.2500 | 437.5000 | 156.2500 |  5083706 B |
|     NetCoreJson | Deserialization |  16.109 ms |  1.16 |  375.0000 | 187.5000 |        - |  2329120 B |
|   XmlSerializer | Deserialization |  13.918 ms |  1.01 |  500.0000 | 296.8750 |  62.5000 |  2715815 B |
|        Utf8Json | Deserialization |   7.004 ms |  0.51 |  468.7500 | 296.8750 | 125.0000 |  2000148 B |
|     MessagePack | Deserialization |   3.381 ms |  0.24 |  550.7813 | 382.8125 | 187.5000 |  2253308 B |
|      **BinaryPack** | Deserialization |   **1.171 ms** |  **0.08** |  **369.1406** | **183.5938** |        - |  2222632 B |

### Large JSON response (~9MB)
|          Method |      Categories |        Mean | Ratio |      Gen 0 |     Gen 1 |    Gen 2 |   Allocated |
|---------------- |---------------- |------------:|------:|-----------:|----------:|---------:|------------:|
|  NewtonsoftJson |   Serialization |    72.31 ms |  1.00 |   857.1429 |         - |        - |   4072600 B |
| BinaryFormatter |   Serialization |    80.40 ms |  1.11 |  1571.4286 |  428.5714 |        - |   7557513 B |
|     NetCoreJson |   Serialization |    85.77 ms |  1.19 |  1833.3333 |         - |        - |   8085480 B |
|   XmlSerializer |   Serialization |    87.49 ms |  1.21 |  5833.3333 |         - |        - |  25062624 B |
|        Utf8Json |   Serialization |    62.59 ms |  0.87 |          - |         - |        - |        72 B |
|     MessagePack |   Serialization |    22.79 ms |  0.32 |   187.5000 |  187.5000 | 187.5000 |       325 B |
|      **BinaryPack** |   Serialization |    **15.71 ms** |  **0.22** |    78.1250 |   78.1250 |  78.1250 |       198 B |
|                 |                 |             |       |            |           |          |             |
|  NewtonsoftJson | Deserialization |   129.27 ms |  1.00 |  3750.0000 | 1500.0000 | 250.0000 |  22250810 B |
| BinaryFormatter | Deserialization |   157.14 ms |  1.22 |  7000.0000 | 2750.0000 | 750.0000 |  39735018 B |
|     NetCoreJson | Deserialization |   140.78 ms |  1.09 |  3000.0000 | 1250.0000 | 250.0000 |  17723686 B |
|   XmlSerializer | Deserialization |   148.37 ms |  1.15 |  4250.0000 | 1750.0000 | 500.0000 |  23398808 B |
|        Utf8Json | Deserialization |    79.97 ms |  0.62 |  3142.8571 | 1428.5714 | 428.5714 |  17421427 B |
|     MessagePack | Deserialization |    42.08 ms |  0.33 |  2937.5000 | 1312.5000 | 437.5000 |  16682502 B |
|      **BinaryPack** | Deserialization |    **29.61 ms** |  **0.23** |  3062.5000 | 1281.2500 | 406.2500 |  17133154 B |

### Neural network layer model
|          Method |      Categories |        Mean | Ratio |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|---------------- |---------------- |------------:|------:|----------:|---------:|---------:|----------:|
|  NewtonsoftJson |   Serialization | 26,552.5 us | 1.000 | 1281.2500 |  93.7500 |  62.5000 | 5217488 B |
| BinaryFormatter |   Serialization |    175.5 us | 0.007 |   70.5566 |  33.9355 |  33.6914 |  155111 B |
|     NetCoreJson |   Serialization | 36,868.3 us | 1.389 | 1071.4286 |  71.4286 |  71.4286 | 4248542 B |
|   XmlSerializer |   Serialization | 31,354.5 us | 1.181 | 1250.0000 | 468.7500 | 437.5000 | 3452059 B |
|        Utf8Json |   Serialization | 13,936.4 us | 0.525 |  109.3750 | 109.3750 | 109.3750 |      72 B |
|     MessagePack |   Serialization |    421.0 us | 0.016 |  111.3281 | 111.3281 | 111.3281 |     762 B |
|      **BinaryPack** |   Serialization |    **108.9 us** | **0.004** |   **10.0098** |  **10.0098** |  **10.0098** |      **96 B** |
|                 |                 |             |       |           |          |          |           |
|  NewtonsoftJson | Deserialization | 25,525.0 us | 1.000 | 1687.5000 |  62.5000 |  31.2500 | 6994104 B |
| BinaryFormatter | Deserialization |    126.4 us | 0.005 |   13.9160 |  10.7422 |  10.7422 |   13247 B |
|     NetCoreJson | Deserialization | 16,771.4 us | 0.657 |   62.5000 |  31.2500 |  31.2500 |  133380 B |
|   XmlSerializer | Deserialization | 38,382.1 us | 1.503 | 1071.4286 | 428.5714 | 285.7143 | 3575022 B |
|        Utf8Json | Deserialization | 17,709.0 us | 0.694 |  187.5000 | 187.5000 | 187.5000 |  101100 B |
|     MessagePack | Deserialization |    978.2 us | 0.039 |   95.7031 |  95.7031 |  95.7031 |    1258 B |
|      **BinaryPack** | Deserialization |    **111.8 us** | **0.004** |   **10.0098** |   **9.8877** |   **9.8877** |     **826 B** |

# Requirements

The **BinaryPack** library requires .NET Standard 2.1 support and it has no external dependencies.

Additionally, you need an IDE with .NET Core 3.0 and C# 8.0 support to compile the library and samples on your PC.

# Special thanks

Icon made by [freepik](https://www.flaticon.com/authors/freepik) from www.flaticon.com.
