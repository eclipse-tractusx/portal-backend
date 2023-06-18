using System.Text.Json;
using System.Text.Json.Serialization;
using Notifications.Service.Tests.RestAssured;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using Xunit;
using static RestAssured.Dsl;

namespace Notifications.Service.Tests.EndToEndTests;

public class EndToEndTests
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/notification";
    private static readonly string AdminEndPoint = "/api/administration";
    private static string? _companyUserId;
    private static string? _techUserToken;
    private static string? _username;
    private static readonly string TechCompanyName = TestResources.TechCompanyName;
    private static readonly string OfferId = TestResources.NotificationOfferId;
    private static readonly Secrets Secrets = new();

    [Fact]
    public async Task Scenario_HappyPathAssignUnassignCoreUserRoles()
    {
        await GetTechUserToken();
        _companyUserId = GetCompanyUserId();

        var assignedRoles = GetUserAssignedRoles();
        var roleToModify = GetRandomRoleToModify(assignedRoles);

        ModifyCoreUserRoles_AssignRole(assignedRoles, roleToModify);
        ModifyCoreUserRoles_UnAssignRole(assignedRoles, roleToModify);
    }

    private async Task GetTechUserToken()
    {
        _techUserToken =
            await new AuthFlow(TechCompanyName).GetAccessToken(Secrets.TechUserName, Secrets.TechUserPassword);
    }

    //GET: api/administration/user/owncompany/roles/coreoffers
    private string GetRandomRoleToModify(List<string> assignedRoles)
    {
        var newRoles = new List<string>();

        var existingRoles = GetCoreOfferRolesNames();
        foreach (var t in existingRoles)
        {
            newRoles.AddRange(from t1 in assignedRoles where t != t1 select t);
        }

        return newRoles.ElementAt(new Random().Next(0, newRoles.Count - 1));
    }

    [Fact]
    private List<string> GetCoreOfferRolesNames()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/user/owncompany/roles/coreoffers")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        var data = DeserializeData<List<OfferRoleInfos>>(response.Content.ReadAsStringAsync().Result);
        if (data == null) throw new Exception("Cannot fetch core user roles");
        var roleNames = new List<string>();
        foreach (var offerRoleInfo in data.Where(t => t.OfferId.ToString() == OfferId))
        {
            roleNames = offerRoleInfo.RoleInfos.Select(t => t.RoleText).ToList();
        }

        return roleNames;
    }

    //PUT: api/administration/user/owncompany/users/{companyUserId}/coreoffers/{offerId}/roles
    private void ModifyCoreUserRoles_AssignRole(List<string> assignedRoles, string roleToModify)
    {
        assignedRoles.Add(roleToModify);

        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Body(body)
            .Put($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{OfferId}/roles")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        Assert.True(CheckNotificationCreated(_username, NotificationTypeId.ROLE_UPDATE_CORE_OFFER, roleToModify,
            ""));
    }

    private void ModifyCoreUserRoles_UnAssignRole(List<string> assignedRoles, string roleToModify)
    {
        assignedRoles.Remove(roleToModify);

        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Body(body)
            .Put($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{OfferId}/roles")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        Assert.True(CheckNotificationCreated(_username, NotificationTypeId.ROLE_UPDATE_CORE_OFFER, "",
            roleToModify));
    }

    //GET: api/administration/user/ownUser
    private string GetCompanyUserId()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/user/ownUser")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        var data = response.Content.ReadAsStringAsync().Result;
        var companyUserDetails = DeserializeData<CompanyUserDetails>(data);
        _username = companyUserDetails.FirstName + " " + companyUserDetails.LastName;
        return companyUserDetails.Company2UserId.ToString();
    }

    //GET: api/administration/user/owncompany/users/{_companyUserId}
    private List<string>? GetUserAssignedRoles()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var assignedRoles = DeserializeData<CompanyUserDetails>(data)?.AssignedRoles.First().UserRoles.ToList();
        return assignedRoles;
    }

    private bool CheckNotificationCreated(string username, NotificationTypeId notificationTypeId,
        string addedRole,
        string removedRole)
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/?page=0&size=10&sorting=DateDesc")
            .StatusCode(200)
            .Extract()
            .Body("$.content");
        var data = DeserializeData<List<NotificationDetailData>>(response.ToString());

        var notificationContent = DeserializeData<NotificationContent>(data.First().Content);
        return data.First().TypeId == notificationTypeId && notificationContent?.username == username &&
               notificationContent.addedRoles == addedRole && notificationContent.removedRoles == removedRole;
    }

    private T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}