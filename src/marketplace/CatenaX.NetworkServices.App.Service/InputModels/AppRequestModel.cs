namespace CatenaX.NetworkServices.Apps.Service.InputModels;

/// <summary>
/// Request Model for App Creation.
/// </summary>
/// <param name="Title">Title</param>
/// <param name="Provider">Provider</param>
/// <param name="LeadPictureUri">LeadPictureUri</param>
/// <param name="ProviderCompanyId">ProviderCompanyId</param>
/// <param name="UseCaseIds">UseCaseIds</param>
/// <param name="Descriptions">Descriptions</param>
/// <param name="SupportedLanguageCodes">SupportedLanguageCodes</param>
/// <param name="Price">Price</param>
/// <returns></returns>

public record AppRequestModel(string? Title, string Provider, string? LeadPictureUri, Guid? ProviderCompanyId, ICollection<string> UseCaseIds, ICollection<LocalizedDescription> Descriptions, ICollection<string> SupportedLanguageCodes, string Price);

