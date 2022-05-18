using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Framework.Models;

public class PaginationResponse<T>
{
    private PaginationResponse(PaginationMetadata meta, IEnumerable<T> content)
    {
        Meta = meta;
        Content = content;
    }

    public static async Task<PaginationResponse<T>> CreatePaginationResponseAsync(int page, int size, int maxSize, Func<Task<int>> getCountAsync, Func<int, int, IAsyncEnumerable<T>> getDataAsync)
    {
        if (page <= 0)
        {
            throw new ArgumentException("parameter page must be > 0", "page");
        }
        if (size > maxSize)
        {
            throw new ArgumentException($"parameter size muse be <= {maxSize}", "size");
        }
        var count = await getCountAsync().ConfigureAwait(false);
        var totalPages = count/size + 1;
        if (page > totalPages)
        {
            throw new ArgumentException($"parameter page must be <= {totalPages}", "page");
        }
        var data = await getDataAsync(size * (page-1), size).ToListAsync().ConfigureAwait(false);

        return new PaginationResponse<T>(
            new PaginationMetadata(
                count,
                totalPages,
                page,
                data.Count()),
            data);
    }

    [JsonPropertyName("meta")]
    public PaginationMetadata Meta { get; set; }

    [JsonPropertyName("content")]
    public IEnumerable<T> Content { get; set; }
}

public class PaginationMetadata
{
    public PaginationMetadata(int numberOfElements, int numberOfPages, int page, int pageSize)
    {
        NumberOfElements = numberOfElements;
        NumberOfPages = numberOfPages;
        Page = page;
        PageSize = pageSize;
    }

    [JsonPropertyName("totalElements")]
    public int NumberOfElements { get; set; }

    [JsonPropertyName("totalPages")]
    public int NumberOfPages { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("contentSize")]
    public int PageSize { get; set; }
}
