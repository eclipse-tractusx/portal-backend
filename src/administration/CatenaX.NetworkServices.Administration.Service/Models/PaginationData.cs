using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class PaginationData
{
    public PaginationData(int numberOfElements, int numberOfPages, int pageSize)
    {
        NumberOfElements = numberOfElements;
        NumberOfPages = numberOfPages;
        PageSize = pageSize;
    }

    [JsonPropertyName("totalElements")]
    public int NumberOfElements { get; set; }

    [JsonPropertyName("totalPages")]
    public int NumberOfPages { get; set; }

    [JsonPropertyName("contentSize")]
    public int PageSize { get; set; }
}
