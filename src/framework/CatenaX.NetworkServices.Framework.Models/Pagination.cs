using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Framework.Models;

public class Pagination
{
    public class Response<T>
    {
        public Response(Metadata meta, IEnumerable<T> content)
        {
            Meta = meta;
            Content = content;
        }

        [JsonPropertyName("meta")]
        public Metadata Meta { get; set; }

        [JsonPropertyName("content")]
        public IEnumerable<T> Content { get; set; }
    }

    public class Metadata
    {
        public Metadata(int numberOfElements, int numberOfPages, int page, int pageSize)
        {
            NumberOfElements = numberOfElements;
            NumberOfPages = numberOfPages;
            Page = page;
            PageSize = pageSize;
        }

        [JsonPropertyName("totalElements")]
        public int NumberOfElements { get; }

        [JsonPropertyName("totalPages")]
        public int NumberOfPages { get; }

        [JsonPropertyName("page")]
        public int Page { get; }

        [JsonPropertyName("contentSize")]
        public int PageSize { get; }
    }

    public class AsyncSource<T>
    {
        public AsyncSource(Task<int> count, IAsyncEnumerable<T> data)
        {
            Count = count;
            Data = data;
        }

        public Task<int> Count { get; }
        public IAsyncEnumerable<T> Data { get; }
    }

    public class Source<T>
    {
        public Source(int count, IEnumerable<T> data)
        {
            Count = count;
            Data = data;
        }

        public int Count { get; }
        public IEnumerable<T> Data { get; }
    }

    public static async Task<Response<T>> CreateResponseAsync<T>(int page, int size, int maxSize, Func<int, int, AsyncSource<T>> getSource)
    {
        ValidatePaginationParameters(page, size, maxSize);

        var source = getSource(size * page, size);
        var count = await source.Count.ConfigureAwait(false);
        var data = await source.Data.ToListAsync().ConfigureAwait(false);

        return new Response<T>(
            new Metadata(
                count,
                count / size + Math.Clamp(count % size, 0, 1),
                page,
                data.Count()),
            data);
    }

    public static async Task<Response<T>?> CreateResponseAsync<T>(int page, int size, int maxSize, Func<int, int, Task<Source<T>?>> getSource)
    {
        ValidatePaginationParameters(page, size, maxSize);

        var source = await getSource(size * page, size).ConfigureAwait(false);
        return source == null
            ? null
            : new Response<T>(
                new Metadata(
                    source.Count,
                    source.Count / size + Math.Clamp(source.Count % size, 0, 1),
                    page,
                    source.Data.Count()),
                source.Data);
    }

    private static void ValidatePaginationParameters(int page, int size, int maxSize)
    {
        if (page < 0)
        {
            throw new ArgumentException("Parameter page must be >= 0", nameof(page));
        }
        if (size <= 0 || size > maxSize)
        {
            throw new ArgumentException($"Parameter size muse be between 1 and {maxSize}", nameof(size));
        }
    }
}
