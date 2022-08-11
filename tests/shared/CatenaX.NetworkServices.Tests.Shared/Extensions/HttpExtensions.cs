using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Tests.Shared.Extensions;

public static class HttpExtensions
{
    public static async Task<T> GetResultFromContent<T>(this HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseString);
    }
}