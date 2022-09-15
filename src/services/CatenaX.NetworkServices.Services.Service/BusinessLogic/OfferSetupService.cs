namespace CatenaX.NetworkServices.Services.Service.BusinessLogic;

public class OfferSetupService : IOfferSetupService
{
    private readonly ILogger _logger;

    public OfferSetupService(ILogger logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<bool> AutoSetupOffer(Guid serviceId, string serviceDetailsAutoSetupUrl)
    {
        var httpClient = new HttpClient();
        
        var requestModel = new OfferAutoSetupData(new CustomerData("", "", ""), new PropertyData("", new Guid(), new Guid()));
        var response = await httpClient.PostAsJsonAsync(serviceDetailsAutoSetupUrl, requestModel).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
}

public record OfferAutoSetupData(CustomerData customer, PropertyData properties);
public record CustomerData(string organizationName, string country, string email);
public record PropertyData(string bpnNumber, Guid subscriptionId, Guid serviceId);
