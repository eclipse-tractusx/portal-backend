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
        if (page <= 0)
        {
            throw new ArgumentException("parameter page must be > 0", "page");
        }
        if (size > maxSize)
        {
            throw new ArgumentException($"parameter size muse be <= {maxSize}", "size");
        }

        var source = getSource(size * (page-1), size);
        var count = await source.Count.ConfigureAwait(false);
        var data = await source.Data.ToListAsync().ConfigureAwait(false);

        return new Response<T>(
            new Metadata(
                count,
                count/size + 1,
                page,
                data.Count()),
            data);
    }

    public static async Task<Response<T>?> CreateResponseAsync<T>(int page, int size, int maxSize, Func<int, int, Task<Source<T>?>> getSource)
    {
        if (page <= 0)
        {
            throw new ArgumentException("parameter page must be > 0", "page");
        }
        if (size > maxSize)
        {
            throw new ArgumentException($"parameter size muse be <= {maxSize}", "size");
        }

        var source = await getSource(size * (page-1), size).ConfigureAwait(false);
        return source == null
            ? null
            : new Response<T>(
                new Metadata(
                    source.Count,
                    source.Count/size + 1,
                    page,
                    source.Data.Count()),
                source.Data);
    }
}
