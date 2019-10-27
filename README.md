![BinaryPackIcon](https://user-images.githubusercontent.com/10199417/67103112-d8852800-f1c4-11e9-9679-8cb344e988dc.png) [![NuGet](https://img.shields.io/nuget/v/BinaryPack.svg?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/BinaryPack/) [![NuGet](https://img.shields.io/nuget/dt/BinaryPack.svg?style=for-the-badge)](https://www.nuget.org/stats/packages/BinaryPack?groupby=Version)
[![AppVeyor](https://img.shields.io/appveyor/ci/Sergio0694/binarypack/master.svg?style=for-the-badge&logo=appveyor)](https://ci.appveyor.com/project/Sergio0694/binarypack/master/)

# What is it?

**BinaryPack** is a binary serialization library similar similar to MessagePack, but producing even smaller files. The goal of this project is to be able to use **BinaryPack** as a drop-in replacement for JSON serialization, when the serialized models don't need to be shared with other applications or with web services. **BinaryPack** is built to be as fast and memory efficient as possible: it uses virtually no memory allocations, and the serialized data is packed to take up as little space as possible.

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [Quick start](#quick-start)
  - [Supported properties](#supported-properties)
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

# Requirements

The **BinaryPack** library requires .NET Standard 2.1 support and it has no external dependencies.

Additionally, you need an IDE with .NET Core 3.0 and C# 8.0 support to compile the library and samples on your PC.

# Special thanks

Icon made by [freepik](https://www.flaticon.com/authors/freepik) from www.flaticon.com.
