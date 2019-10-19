using System;
using System.Linq;
using BinaryPack.Models.Helpers;
using BinaryPack.Models.Interfaces;

namespace BinaryPack.Models
{
    /// <summary>
    /// A model that represents a container for a fake REST API response in JSON format
    /// </summary>
    public sealed class JsonResponseModel : IInitializable, IEquatable<JsonResponseModel>
    {
        public string? Id { get; set; }

        public string? Type { get; set; }

        public int Count { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public string? PreviousPageId { get; set; }

        public string? FollowingPageId { get; set; }

        /// <summary>
        /// A model that represents a container for a fake API response
        /// </summary>
        public sealed class ApiModelContainer : IInitializable, IEquatable<ApiModelContainer>
        {
            public string? Id { get; set; }

            public string? Type { get; set; }

            public RestApiModel? Model { get; set; }

            /// <inheritdoc/>
            public void Initialize()
            {
                Id = RandomProvider.NextString(10);
                Type = nameof(JsonResponseModel);
                Model = new RestApiModel();
                Model.Initialize();
            }

            /// <inheritdoc/>
            public bool Equals(ApiModelContainer? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) throw new InvalidOperationException();
                return
                    Id?.Equals(other.Id) == true &&
                    Type?.Equals(other.Type) == true &&
                    Model?.Equals(other.Model) == true;
            }
        }

        public ApiModelContainer[]? ModelContainers { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(10);
            Type = nameof(JsonResponseModel);
            Count = RandomProvider.NextInt();
            CreationTime = RandomProvider.NextDateTime();
            UpdateTime = RandomProvider.NextDateTime();
            ExpirationTime = RandomProvider.NextDateTime();
            PreviousPageId = RandomProvider.NextString(10);
            FollowingPageId = RandomProvider.NextString(10);
            ModelContainers = new ApiModelContainer[50];
            for (int i = 0; i < ModelContainers.Length; i++)
            {
                ModelContainers[i] = new ApiModelContainer();
                ModelContainers[i].Initialize();
            }
        }

        /// <inheritdoc/>
        public bool Equals(JsonResponseModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Type == other.Type &&
                Count == other.Count &&
                CreationTime.Equals(other.CreationTime) &&
                UpdateTime.Equals(other.UpdateTime) &&
                ExpirationTime.Equals(other.ExpirationTime) &&
                PreviousPageId == other.PreviousPageId &&
                FollowingPageId == other.FollowingPageId &&
                ModelContainers?.Length == other.ModelContainers?.Length &&
                ModelContainers.Zip(other.ModelContainers).All(p => p.First.Equals(p.Second));
        }
    }

    /// <summary>
    /// A model that represents a REST API response for a single item
    /// </summary>
    public sealed class RestApiModel : IInitializable, IEquatable<RestApiModel>
    {
        public string? Id { get; set; }

        public string? Type { get; set; }

        public string? Parent { get; set; }

        public string? Author { get; set; }

        public string? Title { get; set; }

        public string? Text { get; set; }

        public string? Url { get; set; }

        public string? HtmlContent { get; set; }

        public int Upvotes { get; set; }

        public int Downvotes { get; set; }

        public float VotesRatio { get; set; }

        public int Views { get; set; }

        public int Clicks { get; set; }

        public float ClicksRatio { get; set; }

        public int NumberOfComments { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public bool Flag1 { get; set; }

        public bool Flag2 { get; set; }

        public bool Flag3 { get; set; }

        public bool Flag4 { get; set; }

        public bool Flag5 { get; set; }

        public string? Optional1 { get; set; }

        public string? Optional2 { get; set; }

        public string? Optional3 { get; set; }

        /// <summary>
        /// A model that represents a collection of fake images
        /// </summary>
        public sealed class MediaInfoModel : IInitializable, IEquatable<MediaInfoModel>
        {
            public string? Id { get; set; }

            public string? AlbumUrl { get; set; }

            public bool Property { get; set; }

            /// <summary>
            /// A simple model that contains a fake URL to an image and some metadata
            /// </summary>
            public sealed class ImageModel : IInitializable, IEquatable<ImageModel>
            {
                public string? Url { get; set; }

                public int Width { get; set; }

                public int Height { get; set; }

                public float AspectRatio { get; set; }

                /// <inheritdoc/>
                public void Initialize()
                {
                    Url = RandomProvider.NextString(60);
                    Width = RandomProvider.NextInt();
                    Height = RandomProvider.NextInt();
                    AspectRatio = Width / (float)Height;
                }

                /// <inheritdoc/>
                public bool Equals(ImageModel? other)
                {
                    if (other is null) return false;
                    if (ReferenceEquals(this, other)) throw new InvalidOperationException();
                    return
                        Url?.Equals(other.Url) == true &&
                        Width == other.Width &&
                        Height == other.Height &&
                        MathF.Abs(AspectRatio - other.AspectRatio) < 0.001f;
                }
            }

            public ImageModel[]? Images { get; set; }

            /// <inheritdoc/>
            public void Initialize()
            {
                Id = RandomProvider.NextString(10);
                AlbumUrl = RandomProvider.NextString(60);
                Property = RandomProvider.NextBool();
                Images = new ImageModel[RandomProvider.NextInt() % 9 + 1];
                for (int i = 0; i < Images.Length; i++)
                {
                    Images[i] = new ImageModel();
                    Images[i].Initialize();
                }
            }

            /// <inheritdoc/>
            public bool Equals(MediaInfoModel? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return
                    Id?.Equals(other.Id) == true &&
                    AlbumUrl?.Equals(other.AlbumUrl) == true &&
                    Property == other.Property &&
                    Images?.Length == other.Images?.Length &&
                    Images.Zip(other.Images).All(p => p.First.Equals(p.Second));
            }
        }

        public MediaInfoModel? Info { get; set; }

        /// <inheritdoc/>
        public void Initialize()
        {
            Id = RandomProvider.NextString(10);
            Type = nameof(RestApiModel);
            Parent = RandomProvider.NextString(10);
            Author = RandomProvider.NextString(20);
            Title = RandomProvider.NextString(30);
            Text = RandomProvider.NextString(200);
            Url = RandomProvider.NextString(60);
            HtmlContent = RandomProvider.NextString(320);
            Upvotes = RandomProvider.NextInt();
            Downvotes = RandomProvider.NextInt();
            VotesRatio = Upvotes / (float)Downvotes;
            Views = RandomProvider.NextInt();
            Clicks = RandomProvider.NextInt();
            ClicksRatio = Views / (float)Clicks;
            NumberOfComments = RandomProvider.NextInt();
            CreationTime = RandomProvider.NextDateTime();
            UpdateTime = RandomProvider.NextDateTime();
            ExpirationTime = RandomProvider.NextDateTime();
            Flag1 = RandomProvider.NextBool();
            Flag2 = RandomProvider.NextBool();
            Flag3 = RandomProvider.NextBool();
            Flag4 = RandomProvider.NextBool();
            Flag5 = RandomProvider.NextBool();
            if (RandomProvider.NextBool()) Optional1 = RandomProvider.NextString(20);
            if (RandomProvider.NextBool()) Optional2 = RandomProvider.NextString(20);
            if (RandomProvider.NextBool()) Optional3 = RandomProvider.NextString(20);
            if (RandomProvider.NextBool())
            {
                Info = new MediaInfoModel();
                Info.Initialize();
            }
        }

        /// <inheritdoc/>
        public bool Equals(RestApiModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Type == other.Type &&
                Parent == other.Parent &&
                Author == other.Author &&
                Title == other.Title &&
                Text == other.Text &&
                Url == other.Url &&
                HtmlContent == other.HtmlContent &&
                Upvotes == other.Upvotes &&
                Downvotes == other.Downvotes &&
                MathF.Abs(VotesRatio - other.VotesRatio) < 0.001f &&
                Views == other.Views &&
                Clicks == other.Clicks &&
                MathF.Abs(ClicksRatio - other.ClicksRatio) < 0.001f &&
                NumberOfComments == other.NumberOfComments &&
                CreationTime.Equals(other.CreationTime) &&
                UpdateTime.Equals(other.UpdateTime) &&
                ExpirationTime.Equals(other.ExpirationTime) &&
                Flag1 == other.Flag1 &&
                Flag2 == other.Flag2 &&
                Flag3 == other.Flag3 &&
                Flag4 == other.Flag4 &&
                Flag5 == other.Flag5 &&
                Optional1 == other.Optional1 &&
                Optional2 == other.Optional2 &&
                Optional3 == other.Optional3 &&
                Info?.Equals(other.Info) == true;
        }
    }
}
