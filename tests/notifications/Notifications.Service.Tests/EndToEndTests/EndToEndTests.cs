using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Registration.Service.Tests.RestAssured;
using Tests.Shared.RestAssured.AuthFlow;
using Xunit;
using static RestAssured.Dsl;

namespace Notifications.Service.Tests.RestAssured;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests")]
public class EndToEndTests
{
    private static readonly string BaseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string EndPoint = "/api/notification";
    private static readonly string AdminEndPoint = "/api/administration";
    private static string? _companyUserId;
    private static string? _techUserToken;
    private static string? _username;
    private const string TechCompanyName = "TestAutomation";
    private const string OfferId = "9b957704-3505-4445-822c-d7ef80f27fcd";
    private static readonly Secrets Secrets = new ();

    [Theory]
    [MemberData(nameof(GetDataEntries))]
    public async Task Scenario_HappyPathAssignUnassignCoreUserRoles(TestDataModel testEntry)
    {
        await GetTechUserToken();
        _companyUserId = GetCompanyUserId();

        ModifyCoreUserRoles_AssignRole(testEntry.rolesToAssign);
        ModifyCoreUserRoles_UnAssignRole(testEntry.rolesToUnAssign);
    }

    private static IEnumerable<object> GetDataEntries()
    {
        var testDataEntries = TestDataHelper.GetTestData();
        if (testDataEntries == null) throw new Exception("No test data was found");
        foreach (var t in testDataEntries)
        {
            yield return new object[] { t };
        }
    }

    private async Task GetTechUserToken()
    {
        _techUserToken =
            await new AuthFlow(TechCompanyName).GetAccessToken(Secrets.TechUserName, Secrets.TechUserPassword);
    }

    //PUT: api/administration/user/owncompany/users/{companyUserId}/coreoffers/{offerId}/roles
    private void ModifyCoreUserRoles_AssignRole(List<string> rolesToAssign)
    {
        List<string>? assignedRoles = GetUserAssignedRoles();
        List<string> newRoles = new List<string>();
        foreach (var role in rolesToAssign.Where(role => !assignedRoles.Contains(role)))
        {
            assignedRoles.Add(role);
            newRoles.Add(role);
        }

        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        Given()
            .RelaxedHttpsValidation()
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

        Assert.True(CheckNotificationCreated(_username, NotificationTypeId.ROLE_UPDATE_CORE_OFFER, newRoles,
            new List<string>()));
    }

    private void ModifyCoreUserRoles_UnAssignRole(List<string> rolesToUnAssign)
    {
        List<string>? assignedRoles = GetUserAssignedRoles();
        foreach (var role in rolesToUnAssign.Where(role => assignedRoles.Contains(role)))
        {
            assignedRoles.Remove(role);
        }

        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        Given()
            .RelaxedHttpsValidation()
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

        Assert.True(CheckNotificationCreated(_username, NotificationTypeId.ROLE_UPDATE_CORE_OFFER, new List<string>(),
            rolesToUnAssign));
    }

    //GET: api/administration/user/ownUser
    private string GetCompanyUserId()
    {
        var response = Given()
            .RelaxedHttpsValidation()
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
            .RelaxedHttpsValidation()
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
        List<string> addedRoles,
        List<string> removedRoles)
    {
        // Given
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/?page=0&size=10&sorting=DateDesc")
            // .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.content");
        var data = DeserializeData<List<NotificationDetailData>>(response.ToString());

        var notificationContent = DeserializeData<NotificationContent>(data.First().Content);
        return data.First().TypeId == notificationTypeId && notificationContent?.username == username &&
               notificationContent.removedRoles.Split(",", StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x)
                   .SequenceEqual(removedRoles.OrderBy(x => x)) && notificationContent.addedRoles
                   .Split(",", StringSplitOptions.RemoveEmptyEntries)
                   .OrderBy(x => x)
                   .SequenceEqual(addedRoles.OrderBy(x => x));
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