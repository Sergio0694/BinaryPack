using System;
using System.Collections.Generic;
using System.Linq;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;

#nullable enable

namespace BinaryPack.Models
{
    /// <summary>
    /// A model with a collection of <see cref="IList{T}"/> properties
    /// </summary>
    [Serializable]
    public sealed class ListContainerModel : IInitializable, IEquatable<ListContainerModel>
    {
        public string? Id { get; set; }

        public IList<HelloWorldModel?>? HelloWorlds { get; set; }

        public IList<DateTime>? Times { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = nameof(ListContainerModel);
            HelloWorlds = Enumerable.Range(0, 10).Select(i =>
            {
                var model = new HelloWorldModel();
                model.Initialize();
                return model;
            }).ToList();
            Times = Enumerable.Range(0, 10).Select(_ => RandomProvider.NextDateTime()).ToList();
        }

        /// <inheritdoc/>
        public bool Equals(ListContainerModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Id != other.Id) return false;
            if (!(HelloWorlds == null && other.HelloWorlds == null ||
                  HelloWorlds?.GetType() == other.HelloWorlds?.GetType() &&
                  HelloWorlds.Zip(other.HelloWorlds).All(p => p.First?.Equals(p.Second) == true))) return false;
            if (!(Times == null && other.Times == null ||
                  Times?.GetType() == other.Times?.GetType() &&
                  Times.Zip(other.Times).All(p => p.First.Equals(p.Second)))) return false;
            return true;
        }
    }
}

