namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;

public record CompanyAppUsersFilter(int Page, int Size, string? FirstName, string? LastName, string? Email, string? RoleName, bool? HasRole);