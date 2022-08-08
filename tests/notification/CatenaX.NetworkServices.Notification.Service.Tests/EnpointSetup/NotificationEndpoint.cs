using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Tests.Shared.IntegrationTests.EnpointSetup;

namespace CatenaX.NetworkServices.Notification.Service.Tests.EnpointSetup;

public class NotificationEndpoint
{
    private readonly HttpClient _client;

    public static string Path => Paths.Notification;

    public NotificationEndpoint(HttpClient client)
    {
        this._client = client;
    }

    public async Task<HttpResponseMessage> CreateNotification(Guid companyUserId, NotificationCreationData data)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Path}/{companyUserId}");
        request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        return await this._client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetNotification(Guid notificationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/{notificationId}");
        return await this._client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetNotifications(Guid applicationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/application/{applicationId}/companyDetailsWithAddress");
        return await this._client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> NotificationCount(bool? isRead)
    {
        var countParam = isRead.HasValue ? $"?isRead={isRead.Value}" : string.Empty;
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/count{countParam}");
        return await this._client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SetNotificationToRead(Guid notificationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{Path}/{notificationId}/read");
        return await this._client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteNotification(Guid notificationId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{Path}/{notificationId}");
        return await this._client.SendAsync(request);
    }
}