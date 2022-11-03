namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;

public record CompanyAppUsersFilter(int page, int size, string? firstName = null, string? lastName = null, string? email = null, string? roleName = null, bool? hasRole = null);