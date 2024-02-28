/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests.EndpointSetup;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Tests.EndpointSetup;

public class NotificationEndpoints
{
    private readonly HttpClient _client;

    public static string Path => Paths.Notification;

    public NotificationEndpoints(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> CreateNotification(Guid companyUserId, NotificationCreationData data)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Path}/{companyUserId}");
        request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        return await _client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetNotification(Guid notificationId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/{notificationId}");
        return await _client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetNotifications(Guid applicationId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/application/{applicationId}/companyDetailsWithAddress");
        return await _client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> NotificationCount(bool? isRead)
    {
        var countParam = isRead.HasValue ? $"?isRead={isRead.Value}" : string.Empty;
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/count{countParam}");
        return await _client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SetNotificationToRead(Guid notificationId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{Path}/{notificationId}/read");
        return await _client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteNotification(Guid notificationId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Path}/{notificationId}");
        return await _client.SendAsync(request);
    }
}
