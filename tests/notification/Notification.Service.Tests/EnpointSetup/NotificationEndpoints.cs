/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.IntegrationTests.EndpointSetup;

namespace Org.CatenaX.Ng.Portal.Backend.Notification.Service.Tests.EnpointSetup;

public class NotificationEndpoints
{
    private readonly HttpClient _client;

    public static string Path => Paths.Notification;

    public NotificationEndpoints(HttpClient client)
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