using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Expressions;
using Registration.Service.Tests.RestAssured;

namespace Notifications.Service.Tests.RestAssured;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests")]
public class NotificationEndpointTests
{
    private static readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/notification";
    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _regEndPoint = "/api/registration";
    private static string _companyUserId;
    private static readonly string _userToken;
    private const string _techCompanyName = "TestAutomation";
    private static string _username;
    private static string _notificationId = "";
    private const string _offerId = "9b957704-3505-4445-822c-d7ef80f27fcd";
    private static readonly Secrets _secrets = new ();
    private static string _techUserToken; 
    private const string _testDataDirectory = "..\\..\\..\\..\\..\\shared\\Tests.Shared\\RestAssured\\TestData";

    //PUT: api/administration/user/owncompany/users/{companyUserId}/coreoffers/{offerId}/roles
    [Fact]
    public async Task ModifyCoreUserRoles_AssignRole_ReturnsExpectedResult()
    {
        _techUserToken = await new AuthFlow(_techCompanyName).GetAccessToken(_secrets.TechUserName, _secrets.TechUserPassword);
        _companyUserId = GetCompanyUserId();
        string newRole = "App Manager";
        List<string> assignedRoles = GetUserAssignedRoles();
        assignedRoles.Add("App Manager");
        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Body(body)
            .Put($"{_baseUrl}{_adminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{_offerId}/roles")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        Assert.True(CheckNotificationCreated(_username, "ROLE_UPDATE_CORE_OFFER", newRole, ""));
    }
    
    [Fact]
    public void ModifyCoreUserRoles_UnAssignRole_ReturnsExpectedResult()
    {
        string removedRole = "App Manager";
        List<string> assignedRoles = GetUserAssignedRoles();
        assignedRoles.Remove("App Manager");
        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Body(body)
            .Put($"{_baseUrl}{_adminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{_offerId}/roles")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        
        Assert.True(CheckNotificationCreated(_username, "ROLE_UPDATE_CORE_OFFER", "", removedRole));
    }
    
    
    [Fact]
    public void Test1_CreateNotification_ReturnsExpectedResult()
    {
        // Given
        var creationData = new
            NotificationCreationData("test", NotificationTypeId.INFO, false);
        //     { Content = "test", NotificationTypeId = NotificationTypeId.INFO, IsRead = false };
        _notificationId = (string)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .ContentType("application/json")
            .Body(
                "{\"content\": \"test\",\"notificationTypeId\": \"INFO\",\"isRead\": false}")
            // .Body(creationData)
            .When()
            .Post($"{_baseUrl}{_endPoint}?companyUserId={_companyUserId}")
            .Then()
            .StatusCode(201)
            .Extract()
            .As(typeof(string));
    }

    [Fact]
    public void Test2_GetNotifications_ReturnsExpectedResult()
    {
        // Given
        Pagination.Response<NotificationDetailData> data = (Pagination.Response<NotificationDetailData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(Pagination.Response<NotificationDetailData>));
        Assert.Contains(_notificationId, data.Content.Select(content => content.Id.ToString()));
    }

    [Fact]
    public void Test3_GetNotification_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/{_notificationId}")
            .Then()
            .StatusCode(200);
    }

    [Fact]
    public void Test4_NotificationCount_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/count")
            .Then()
            .Body("1")
            .StatusCode(200);
    }

    [Fact]
    public void Test5_NotificationCountDetails_ReturnsExpectedResult()
    {
        // Given
        var notificationCountDetails = (NotificationCountDetails)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/count-details")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(NotificationCountDetails));
        Assert.Equal(1, notificationCountDetails.Unread);
        Assert.Equal(1, notificationCountDetails.InfoUnread);
        Assert.Equal(0, notificationCountDetails.OfferUnread);
        Assert.Equal(0, notificationCountDetails.ActionRequired);
    }

    [Fact]
    public void Test6_SetNotificationStatus_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Put($"{_baseUrl}{_endPoint}/{_notificationId}/read")
            .Then()
            .StatusCode(204);
    }

    [Fact]
    public void Test7_DeleteNotification_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userToken}")
            .When()
            .Delete($"{_baseUrl}{_endPoint}/{_notificationId}")
            .Then()
            .StatusCode(204);
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
    
    //GET: api/administration/user/ownUser
    [Fact]
    private string GetCompanyUserId()
    {
         var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/user/ownUser")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        
        var data = response.Content.ReadAsStringAsync().Result;
        var companyUserDetails = DeserializeData<CompanyUserDetails>(data);
        _username = companyUserDetails.FirstName + " " + companyUserDetails.LastName; 
        return companyUserDetails.companyUserId.ToString();
    }

    //GET: api/administration/user/owncompany/roles/coreoffers
    [Fact]
    private void GetCoreOfferRoles_ReturnsExpectedResult()
    {
         var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/user/owncompany/roles/coreoffers")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
    }
    
    //GET: api/administration/user/owncompany/users/{_companyUserId}
    [Fact]
    private List<string> GetUserAssignedRoles()
    {
          var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/user/owncompany/users/{_companyUserId}")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var assignedRoles = DeserializeData<CompanyUserDetails>(data).assignedRoles.First().UserRoles.ToList();
        return assignedRoles;
    }
    
    
    private bool CheckNotificationCreated(string username, string notificationTypeId, string addedRoles, string removedRoles)
    {
        // Given
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_techUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.content");
            //.Response();
            var data = DeserializeData<List<NotificationDetailData>>(response.ToString());

            var notificationContent = DeserializeData<NotificationContent>(data.First().Content);
            if (data.First().TypeId.ToString() == notificationTypeId && notificationContent.username == username && notificationContent.removedRoles == removedRoles && notificationContent.addedRoles == addedRoles) return true;
            return false;
    }
    
    [Fact]
    public List<string> GetTestData()
    {
        var filePath = Path.Combine(_testDataDirectory, "HappyPathModifyCoreUserRoles.json");
        var jsonData = File.ReadAllText(filePath);
        var testData = JsonSerializer.Deserialize<List<string>>(jsonData);

        return testData;
    }
    
}