using System.Net.Http.Headers;
using System.Web;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class DapsService : IDapsService
{
    private const string BaseSecurityProfile = "BASE_SECURITY_PROFILE";
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a new instance of <see cref="DapsService"/>
    /// </summary>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    /// <param name="options">The options</param>
    public DapsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(DapsService));
    }

    /// <inheritdoc />
    public async Task<bool> EnableDapsAuthAsync(string clientName, string accessToken, string connectorUrl, string businessPartnerNumber, IFormFile formFile, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (!connectorUrl.StartsWith("http://") && !connectorUrl.StartsWith("https://"))
        {
            throw new ArgumentException("The ConnectorUrl must either start with http:// or https://", nameof(connectorUrl));
        }

        try
        {
            var multiPartStream = new MultipartFormDataContent();
        
            using var stream = formFile.OpenReadStream();
            multiPartStream.Add(new StreamContent(stream), "file", formFile.FileName);

            multiPartStream.Add(new StringContent(clientName), "clientName");
            multiPartStream.Add(new StringContent(BaseSecurityProfile), "securityProfile");
            multiPartStream.Add(new StringContent(HttpUtility.UrlEncode(Path.Combine(connectorUrl, businessPartnerNumber))), "referringConnector");

            var response = await _httpClient.PostAsync(string.Empty, multiPartStream, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceException("Daps Service Call failed", response.StatusCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException("Daps Service Call failed", ex);
        }
    }
}
