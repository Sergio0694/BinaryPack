![BinaryPackIcon](https://user-images.githubusercontent.com/10199417/67103112-d8852800-f1c4-11e9-9679-8cb344e988dc.png) [![NuGet](https://img.shields.io/nuget/v/BinaryPack.svg?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/BinaryPack/) [![NuGet](https://img.shields.io/nuget/dt/BinaryPack.svg?style=for-the-badge)](https://www.nuget.org/stats/packages/BinaryPack?groupby=Version)
[![AppVeyor](https://img.shields.io/appveyor/ci/Sergio0694/binarypack/master.svg?style=for-the-badge&logo=appveyor)](https://ci.appveyor.com/project/Sergio0694/binarypack/master/)

**BinaryPack** is a binary serialization library inspired by `MessagePack`, but even faster, more efficient and producing smaller files. The goal of this project is to be able to use **BinaryPack** as a drop-in replacement for JSON, XML, `MessagePack` or `BinaryFormatter` serialization, when the serialized models don't need to be shared with other applications or with web services. **BinaryPack** is built to be as fast and memory efficient as possible: it uses virtually no memory allocations, and the serialized data is packed to take up as little space as possible. Whenever you're using either JSON, `MessagePack`, XML or some other format to cache data for your apps, to send data between clients or to save data that is not critical, you can try using **BinaryPack** over your previous serialization library - it will provide the same basic functionalities for models serialization, but with much higher performance and less memory usage.

> **DISCLAIMER:** this library is provided as is, and it's no longer being actively maintained. **BinaryPack** was developed just for fun and it has not been tested in critical production environments. It does work fine in the scenarios described in this document (eg. I'm using this library to handle local cache files in some of my apps), but if you're looking for a more widely used and well tested library for fast binary serialization (that also offers better flexibility and customization), I'd recommend looking into [`MessagePack-CSharp`](https://github.com/neuecc/MessagePack-CSharp) first.

![BinaryPack-benchmark](https://i.imgur.com/WJYuBXK.png)

This benchmark was performed with the [JsonResponseModel](https://github.com/Sergio0694/BinaryPack/blob/master/unit/BinaryPack.Models/JsonResponseModel.cs) class available in the repository, which contains a number of `string`, `int`, `double` and `DateTime` properties, as well as a collection of other nested models, representing an example of a JSON response from a REST API. This README also includes a number of benchmarks that were performed on a number of different models. The benchmark code and all the models used can be found in this repository as well. To summarize:
- **BinaryPack** was consistently the fastest library, both during serialization and deserialization. The performance difference ranged from **7.6x** faster than `Newtonsoft.Json`, **7x** than [`Utf8Json`](https://github.com/neuecc/Utf8Json) and **1.9x** than `MessagePack` when serializing a small JSON response, to **245x** faster than `Newtonsoft.Json`, **129x** than `Utf8Json` and **3.9x** than `MessagePack` when dealing with mostly binary data (eg. a model with a large `float[]` array).
- The memory usage was on average on par or better than `Utf8Json`, except when deserializing mostly binary data, in which case **BinaryPack** used **1/100** the memory of `Utf8Json`, and **1/2** that of `MessagePack`. **BinaryPack** also almost always resulted in the lowest number of GC collections during serialization and deserialization of models.
- In all cases, the **BinaryPack** serialization resulted in the smallest file on disk.

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [Quick start](#quick-start)
  - [Supported properties](#supported-properties)
  - [Attributes](#attributes)
  - [FAQ](#faq)
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

✅ .NET arrays: `T[]`, `T[,]`, `T[,,]`, etc.

✅ .NET collections: `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, etc.

✅ .NET dictionaries: `Dictionary<TKey, TValue>`, `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`

✅ Other .NET types: `BitArray`

## Attributes
**BinaryPack** has a series of attributes that can be used to customize how the `BinaryConverter` class handles the serialization of input objects. By default, it will serialize all public properties of a type, but this behavior can be changed by using the `BinarySerialization` attribute. Here's an example:

```C#
[BinarySerialization(SerializationMode.Properties | SerializationMode.NonPublicMembers)]
public class MyModel
{
    internal string Id { get; set; }    
    
    public int Valud { get; set; }    
    
    [IgnoredMember]
    public DateTime Timestamp { get; set; }
}
```

Similarly, there's also a `SerializableMember` that can be used when the mode is set to `SerializationMode.Explicit`.

## FAQ

#### Why is this library faster than the competition?

> There are a number of reasons for this. First of all, **BinaryPack** dynamically generates code to serialize and deserialize every type you need. This means that it doesn't need to inspect types using reflection while serializing/deserializing, eg. to see what fields it needs to read etc. - it just creates the right methods once that work directly on instances of each type, and read/write members one after the other exactly as you would do if you were to write that code manually. This also allows **BinaryPack** to have some extremely optimized code paths that would otherwise be completely impossible. Then, unlike the JSON/XML/MessagePack formats, **BinaryPack** doesn't need to include any additional metadata for the serialized items, which saves time. This allows it to use the minimum possible space to serialize every value, which also makes the serialized files as small as possible.

#### Are there some downsides with this approach?

> Yes, skipping all the metadata means that the **BinaryPack** format is not partcularly resilient to changes. This means that if you add or remove one of the serialized members of a type, it will not be possible to read previously serialized instances of that model. Because of this, **BinaryPack** should not be used with important data and is best suited for caching models or for quick serialization of data being exhanged between different clients.

#### Is this compatible with UWP?

> Unfortunately not at the moment, UWP is still on .NET Standard 2.0 and doesn't support dynamic code generation due to how the .NET Native compiler is implemented. Hopefully it will be possible to use **BinaryPack** on UWP when it moves to .NET 5 and the new MonoAOT compiler in the second half of 2020.

# Benchmarks
Here are three full benchmarks executed with the benchmark sample included in this repository. The error and standard deviation columns have been removed to fit each table in the horizontal space available for the README file reader on GitHub. As mentioned before, the JSON response model used in the first two benchmarks is the [JsonResponseModel](https://github.com/Sergio0694/BinaryPack/blob/master/unit/BinaryPack.Models/JsonResponseModel.cs) class. The class used in the last benchmark is instead [NeuralNetworkLayerModel](https://github.com/Sergio0694/BinaryPack/blob/master/unit/BinaryPack.Models/NeuralNetworkLayerModel.cs).

### JSON response

|          Method |      Categories |        Mean | Ratio |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|---------------- |---------------- |------------:|------:|---------:|---------:|---------:|----------:|
|  NewtonsoftJson |   Serialization |  1.083.1 us |  1.00 | 156.2500 | 121.0938 | 107.4219 |  205083 B |
| BinaryFormatter |   Serialization |  1.446.6 us |  1.34 | 132.8125 |  68.3594 |  37.1094 |  402558 B |
|     NetCoreJson |   Serialization |  1.147.0 us |  1.06 | 199.2188 | 142.5781 | 140.6250 |  252407 B |
|   XmlSerializer |   Serialization |  1.274.5 us |  1.18 | 250.0000 | 146.4844 | 107.4219 |  604205 B |
|        Utf8Json |   Serialization |    744.4 us |  0.69 | 140.6250 | 140.6250 | 140.6250 |     495 B |
|     MessagePack |   Serialization |    217.3 us |  0.20 |  61.0352 |  61.0352 |  61.0352 |     432 B |
|      **BinaryPack** |   Serialization |    **168.1 us** |  **0.16** |  **26.6113** |  **26.6113** |  **26.6113** |     **108 B** |
|                 |                 |             |       |          |          |          |           |
|  NewtonsoftJson | Deserialization |  2.092.1 us |  1.00 |  66.4063 |  19.5313 |        - |  304320 B |
| BinaryFormatter | Deserialization |  1.466.9 us |  0.70 | 130.8594 |  48.8281 |        - |  676136 B |
|     NetCoreJson | Deserialization |  1.964.5 us |  0.94 |  50.7813 |  15.6250 |        - |  220856 B |
|   XmlSerializer | Deserialization |  2.098.6 us |  1.00 | 132.8125 |  70.3125 |  35.1563 |  461000 B |
|        Utf8Json | Deserialization |    887.0 us |  0.42 | 165.0391 | 131.8359 | 109.3750 |  237159 B |
|     MessagePack | Deserialization |    337.0 us |  0.16 |  87.4023 |  53.2227 |  35.1563 |  241462 B |
|      **BinaryPack** | Deserialization |    **168.8 us** |  **0.08** |  **46.6309** |  **13.9160** |        - |  **215192 B** |

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
