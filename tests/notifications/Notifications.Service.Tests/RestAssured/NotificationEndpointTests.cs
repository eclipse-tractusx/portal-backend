using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Xunit;
using static RestAssured.Dsl;

namespace Notifications.Service.Tests.RestAssured;

public class NotificationEndpointTests
{
    private readonly IFixture _fixture;
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/notification";
    private readonly Guid _companyUserId;
    private readonly string _token;

    // private readonly string _notificationId;

    public NotificationEndpointTests()
    {
        _fixture = new Fixture();
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<NotificationEndpointTests>()
            .Build();
        _companyUserId = new (configuration.GetValue<string>("CompanyUserId"));
        _token = configuration.GetValue<string>("token");
    }

    [Fact]
    public void CreateNotification_ReturnsExpectedResult()
    {
        // Given
        var creationData = _fixture.Create<NotificationCreationData>();

        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .ContentType("application/json")
            .Body(creationData)
            .When()
            .Post($"{_baseUrl}{_endPoint}/{_companyUserId}")
            .Then()
            .Body("")
            .StatusCode(201);
    }

    [Fact]
    public void GetNotifications_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .Body("{\"meta\":{\"totalElements\":0,\"totalPages\":0,\"page\":0,\"contentSize\":0},\"content\":[]}")
            .StatusCode(200);
    }

    [Fact]
    public void GetNotification_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/notification-id")
            .Then()
            .StatusCode(200);
    }

    [Fact]
    public void NotificationCount_ReturnsExpectedResult()
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
            .Body("0")
            .StatusCode(200);
    }

    [Fact]
    public void NotificationCountDetails_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/count-details")
            .Then()
            .Body("{\"read\":0,\"unread\":0,\"infoUnread\":0,\"offerUnread\":0,\"actionRequired\":0}")
            .StatusCode(200);
    }

    [Fact]
    public void SetNotificationStatus_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .When()
            .Put($"{_baseUrl}{_endPoint}/read/f22f2b57-426a-4ac3-b3af-7924a1c61590")
            .Then()
            .StatusCode(204);
    }

    [Fact]
    public void DeleteNotification_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_token}")
            .When()
            .Delete($"{_baseUrl}{_endPoint}/f22f2b57-426a-4ac3-b3af-7924a1c615901")
            .Then()
            .StatusCode(204);
    }

    // [Fact]
    // public void CompanyDataOwnCompanyDetails()
    // {
    //     // Given
    //     CompanyAddressDetailData companyAddressDetailData = (CompanyAddressDetailData)Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {token}")
    //         .When()
    //         .Get($"{baseUrl}/api/administration/companydata/ownCompanyDetails")
    //         .Then()
    //         .StatusCode(200)
    //         .Extract()
    //         .As(typeof(CompanyAddressDetailData));
    //     // companyId = companyAddressDetailData.CompanyId;
    //     Assert.Matches(companyId.ToString(), companyAddressDetailData.CompanyId.ToString());
    // }

    // [Fact]
    // public void GetOwnUserDetails()
    // {
    //     // Given
    //     CompanyUserDetails companyUserDetails = (CompanyUserDetails)Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_token}")
    //         .When()
    //         .Get($"{_baseUrl}/api/administration/user/ownUser")
    //         .Then()
    //         .StatusCode(200)
    //         .Extract()
    //         .As(typeof(CompanyUserDetails));
    //     // companyId = companyAddressDetailData.CompanyId;
    //     Assert.Matches("a", companyUserDetails.companyUserId.ToString());
    // }
}