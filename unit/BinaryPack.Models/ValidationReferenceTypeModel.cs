using System;
using System.Collections.Generic;
using System.Linq;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A model with all the supported types, used to test all the processors at the same time
    /// </summary>
    public sealed class ValidationReferenceTypeModel : IInitializable, IEquatable<ValidationReferenceTypeModel>
    {
        public bool P1 { get; set; }

        public byte P2 { get; set; }

        public int P3 { get; set; }

        public float P4 { get; set; }

        public double P5 { get; set; }

        public Guid P6 { get; set; }

        public ValidationValueTypeModel P7 { get; set; }

        public bool? PN1_0 { get; set; }

        public bool? PN1_1 { get; set; }

        public byte? PN2 { get; set; }

        public int? PN3_0 { get; set; }

        public int? PN3_1 { get; set; }

        public float? PN4 { get; set; }

        public double? PN5 { get; set; }

        public Guid? PN6 { get; set; }

        public ValidationValueTypeModel? PN7_0 { get; set; }

        public ValidationValueTypeModel? PN7_1 { get; set; }

        public string? P8_0 { get; set; }

        public string? P8_1 { get; set; }

        public string? P8_2 { get; set; }

        public int[]? P9_0 { get; set; }

        public int[]? P9_1 { get; set; }

        public long[]? P10 { get; set; }

        public string?[]? P11_0 { get; set; }

        public string?[]? P11_1 { get; set; }
        
        public IList<int>? P12_0 { get; set; }

        public IList<int>? P12_1 { get; set; }

        public IList<int?>? PN12 { get; set; }

        public IList<string?>? P13 { get; set; }

        public Dictionary<int, JsonResponseModel?>? P14 { get; set; }

        public Dictionary<int, JsonResponseModel?>? P15 { get; set; }

        public Dictionary<int, JsonResponseModel?>? P16 { get; set; }

        public Dictionary<int, DateTime?>? P17 { get; set; }

        public Dictionary<string, int?>? P18 { get; set; }

        public IDictionary<string, int?>? P19 { get; set; }

        public IDictionary<string, JsonResponseModel?>? P20 { get; set; }

        public IReadOnlyDictionary<string, int?>? P21 { get; set; }

        public IReadOnlyDictionary<string, JsonResponseModel?>? P22 { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            P1 = true;
            P2 = (byte)(RandomProvider.NextInt() % byte.MaxValue);
            P3 = RandomProvider.NextInt();
            P4 = 3.14f;
            P5 = 6.28;
            P6 = Guid.NewGuid();
            P7 = new ValidationValueTypeModel();
            P7.Initialize();
            PN1_0 = null;
            PN1_1 = true;
            PN2 = 127;
            PN3_0 = null;
            PN3_1 = RandomProvider.NextInt();
            PN4 = 3.14f;
            PN5 = 6.28;
            PN6 = Guid.NewGuid();
            PN7_0 = null;
            PN7_1 = new ValidationValueTypeModel();
            PN7_1.Value.Initialize();
            P8_0 = null;
            P8_1 = string.Empty;
            P8_2 = RandomProvider.NextString(100);
            P9_0 = null;
            P9_1 = Enumerable.Range(0, 128).ToArray();
            P10 = Enumerable.Range(0, 128).Select(_ => unchecked(long.MaxValue * RandomProvider.NextInt())).ToArray();
            P11_0 = null;
            P11_1 = Enumerable.Range(0, 128).Select(i => i % 2 == 0 ? RandomProvider.NextString(i + 10) : null).ToArray();
            P12_0 = null;
            P12_1 = Enumerable.Range(0, 128).Select(_ => RandomProvider.NextInt()).ToList();
            PN12 = Enumerable.Range(0, 145).Select(i => i % 2 == 0 ? RandomProvider.NextInt() : default(int?)).ToList();
            P13 = Enumerable.Range(0, 128).Select(i => i % 2 == 0 ? RandomProvider.NextString(i + 10) : null).ToArray();
            P14 = null;
            P15 = new Dictionary<int, JsonResponseModel?>();
            P16 = new Dictionary<int, JsonResponseModel?>
            {
                [0] = new JsonResponseModel(true),
                [17] = new JsonResponseModel(true),
                [144] = null,
                [145] = new JsonResponseModel(true)
            };
            P17 = new Dictionary<int, DateTime?>
            {
                [0] = null,
                [1] = DateTime.MaxValue,
                [2] = DateTime.Now,
                [99] = null,
                [243894234] = DateTime.UtcNow
            };
            P18 = null;
            P19 = new Dictionary<string, int?> { ["Hello world"] = 7 };
            P20 = new Dictionary<string, JsonResponseModel?>
            {
                ["Hello"] = new JsonResponseModel(true),
                ["World"] = new JsonResponseModel(true),
                ["!"] = new JsonResponseModel(true)
            };
            P21 = new Dictionary<string, int?> { ["Hello world 2"] = 14 };
            P22 = new Dictionary<string, JsonResponseModel?>
            {
                ["Hello"] = new JsonResponseModel(true),
                ["World"] = new JsonResponseModel(true),
                ["2!"] = new JsonResponseModel(true)
            };
        }

        /// <inheritdoc/>
        public bool Equals(ValidationReferenceTypeModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                P1 == other.P1 &&
                P2 == other.P2 &&
                P3 == other.P3 &&
                MathF.Abs(P4 - other.P4) < 0.001f &&
                Math.Abs(P5 - other.P5) < 0.001 &&
                P6.Equals(other.P6) &&
                P7.Equals(other.P7) &&
                PN1_0 == other.PN1_0 &&
                PN1_1 == other.PN1_1 &&
                PN2 == other.PN2 &&
                PN3_0 == other.PN3_0 &&
                PN3_1 == other.PN3_1 &&
                (PN4.HasValue && other.PN4.HasValue && MathF.Abs(PN4.Value - other.PN4.Value) < 0.001f ||
                 PN4 == null && other.PN4 == null) &&
                (PN5.HasValue && other.PN5.HasValue && Math.Abs(PN5.Value - other.PN5.Value) < 0.001 ||
                 PN5 == null && other.PN5 == null) &&
                (PN6.HasValue && other.PN6.HasValue && PN6.Value.Equals(other.PN6.Value) ||
                 PN6 == null && other.PN6 == null) &&
                (PN7_0.HasValue && other.PN7_0.HasValue && PN7_0.Value.Equals(other.PN7_0.Value) ||
                 PN7_0 == null && other.PN7_0 == null) &&
                (PN7_1.HasValue && other.PN7_1.HasValue && PN7_1.Value.Equals(other.PN7_1.Value) ||
                 PN7_1 == null && other.PN7_1 == null) &&
                (P8_0 != null && other.P8_0 != null && P8_0.Equals(other.P8_0) ||
                 P8_0 == null && other.P8_0 == null) &&
                (P8_1 != null && other.P8_1 != null && P8_1.Equals(other.P8_1) ||
                 P8_1 == null && other.P8_1 == null) &&
                (P8_2 != null && other.P8_2 != null && P8_2.Equals(other.P8_2) ||
                 P8_2 == null && other.P8_2 == null) &&
                (P9_0 != null && other.P9_0 != null && P9_0.Length == other.P9_0.Length && P9_0.Zip(other.P9_0).All(t => t.First == t.Second) ||
                 P9_0 == null && other.P9_0 == null) &&
                (P9_1 != null && other.P9_1 != null && P9_1.Length == other.P9_1.Length && P9_1.Zip(other.P9_1).All(t => t.First == t.Second) ||
                 P9_1 == null && other.P9_1 == null) &&
                (P10 != null && other.P10 != null && P10.Length == other.P10.Length && P10.Zip(other.P10).All(t => t.First == t.Second) ||
                 P10 == null && other.P10 == null) &&
                (P11_0 != null && other.P11_0 != null && P11_0.Length == other.P11_0.Length && P11_0.Zip(other.P11_0).All(t => t.First == t.Second) ||
                 P11_0 == null && other.P11_0 == null) &&
                (P11_1 != null && other.P11_1 != null && P11_1.Length == other.P11_1.Length && P11_1.Zip(other.P11_1).All(t => t.First == t.Second) ||
                 P11_1 == null && other.P11_1 == null) &&
                (P12_0 != null && other.P12_0 != null && P12_0.Count == other.P12_0.Count && P12_0.Zip(other.P12_0).All(t => t.First == t.Second) ||
                 P12_0 == null && other.P12_0 == null) &&
                (P12_1 != null && other.P12_1 != null && P12_1.Count == other.P12_1.Count && P12_1.Zip(other.P12_1).All(t => t.First == t.Second) ||
                 P12_1 == null && other.P12_1 == null) &&
                (PN12 != null && other.PN12 != null && PN12.Count == other.PN12.Count && PN12.Zip(other.PN12).All(t => t.First == t.Second) ||
                 PN12 == null && other.PN12 == null) &&
                (P13 != null && other.P13 != null && P13.Count == other.P13.Count && P13.Zip(other.P13).All(t => t.First == t.Second) ||
                 P13 == null && other.P13 == null) &&
                StructuralComparer.IsMatch(P14, other.P14) &&
                StructuralComparer.IsMatch(P15, other.P15) &&
                StructuralComparer.IsMatch(P16, other.P16) &&
                StructuralComparer.IsMatch(P17, other.P17) &&
                StructuralComparer.IsMatch(P18, other.P18) &&
                StructuralComparer.IsMatch(P19, other.P19) &&
                StructuralComparer.IsMatch(P20, other.P20) &&
                StructuralComparer.IsMatch(P21 as Dictionary<string, int?>, other.P21 as Dictionary<string, int?>) &&
                StructuralComparer.IsMatch(P22 as Dictionary<string, JsonResponseModel?>, other.P22 as Dictionary<string, JsonResponseModel?>);
        }
    }

    /// <summary>
    /// A value type model that does not respect the <see langword="unmanaged"/> constraint, used to help test the <see cref="ValidationReferenceTypeModel"/> type
    /// </summary>
    public struct ValidationValueTypeModel : IInitializable, IEquatable<ValidationValueTypeModel?>
    {
        public bool P1 { get; set; }

        public Guid P2 { get; set; }

        public int[]? P3 { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            P1 = true;
            P2 = Guid.NewGuid();
            P3 = Enumerable.Range(0, 128).ToArray();
        }

        /// <inheritdoc/>
        public bool Equals(ValidationValueTypeModel? other)
        {
            if (!other.HasValue) return false;
            return
                P1 == other.Value.P1 &&
                P2.Equals(other.Value.P2) &&
                (P3 == null == (other.Value.P3 == null) ||
                 P3?.Zip(other.Value.P3).All(t => t.First == t.Second) == true);
        }
    }
}
