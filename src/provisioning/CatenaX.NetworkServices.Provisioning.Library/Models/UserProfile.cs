namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public record UserProfile(string UserName, string? FirstName, string? LastName, string Email, string? Password = null);

