using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public record IdentityProviderMapperModel(string Id, string Name, IdentityProviderMapperType Type, IDictionary<string, object> Config);