namespace CatenaX.NetworkServices.Administration.Service.Models;

public record IdentityProviderUpdateStats(int Updated, int Unchanged, int Error, int Total, IEnumerable<string> Errors);
