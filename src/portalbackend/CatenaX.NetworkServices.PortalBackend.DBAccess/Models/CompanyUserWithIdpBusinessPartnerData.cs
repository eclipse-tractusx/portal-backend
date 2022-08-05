using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public record CompanyUserWithIdpBusinessPartnerData(
    [property: JsonPropertyName("companyUser")] CompanyUser CompanyUser,
    [property: JsonPropertyName("iamIdpAlias")] string IamIdpAlias,
    [property: JsonPropertyName("businessPartnerNumbers")] IEnumerable<string> BusinessPartnerNumbers,
    [property: JsonPropertyName("assignedRoles")] IEnumerable<CompanyUserAssignedRoleDetails> AssignedRoles);
