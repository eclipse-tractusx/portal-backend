using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;

namespace Notifications.Service.Tests.RestAssured;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests")]
public class NotificationEndpointTests
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/notification";
    private readonly Guid _companyUserId;
    private readonly string _token;
    private static string _notificationId = "";

    public NotificationEndpointTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyUserId = new (configuration.GetValue<string>("Secrets:CompanyUserId"));
        _token = configuration.GetValue<string>("Secrets:UserToken");
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
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
                $"Bearer {_token}")
            .When()
            .Delete($"{_baseUrl}{_endPoint}/{_notificationId}")
            .Then()
            .StatusCode(204);
    }
}