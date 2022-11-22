using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class DapsService : IDapsService
{
    private const string BaseSecurityProfile = "BASE_SECURITY_PROFILE";
    private readonly HttpClient _httpClient;
    private readonly DapsSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="DapsService"/>
    /// </summary>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    /// <param name="options">The options</param>
    public DapsService(
        IOptions<DapsSettings> options, 
        IHttpClientFactory httpClientFactory)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient(nameof(DapsService));
    }

    /// <inheritdoc />
    public async Task<bool> EnableDapsAuthAsync(string clientName, string accessToken, string referringConnector, IFormFile formFile, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var multiPartStream = new MultipartFormDataContent();
        
        using var stream = formFile.OpenReadStream();
        multiPartStream.Add(new StreamContent(stream), formFile.Name, formFile.FileName);
        
        multiPartStream.Add(new StringContent(clientName), "clientName");
        multiPartStream.Add(new StringContent(BaseSecurityProfile), "securityProfile");
        multiPartStream.Add(new StringContent(referringConnector), "referringConnector");
        var response = await _httpClient.PostAsync(_settings.DapsUrl, multiPartStream, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}