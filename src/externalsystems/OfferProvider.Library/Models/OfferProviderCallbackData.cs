using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;

/// <summary>
/// Callback data for the offer provider after the auto setup succeeded
/// </summary>
/// <param name="TechnicalUserInfo">Object containing the information of the technical user</param>
/// <param name="ClientInfo">Information of the created client</param>
public record OfferProviderCallbackData(
    [property: JsonPropertyName("technicalUserInfo")] CallbackTechnicalUserInfoData? TechnicalUserInfo,
    [property: JsonPropertyName("clientInfo")] CallbackClientInfoData? ClientInfo
);

/// <summary>
/// Technical User information
/// </summary>
/// <param name="TechnicalUserId">Id of the created technical user</param>
/// <param name="TechnicalUserSecret">User secret for the created user</param>
/// <param name="TechnicalClientId">User secret for the created user</param>
public record CallbackTechnicalUserInfoData(
    [property: JsonPropertyName("technicalUserId")] Guid TechnicalUserId,
    [property: JsonPropertyName("technicalUserSecret")] string? TechnicalUserSecret,
    [property: JsonPropertyName("technicalClientId")] string? TechnicalClientId);

/// <summary>
/// Client infos
/// </summary>
/// <param name="ClientId">Id of the created client</param>
public record CallbackClientInfoData(
    [property: JsonPropertyName("clientId")] string ClientId
);
