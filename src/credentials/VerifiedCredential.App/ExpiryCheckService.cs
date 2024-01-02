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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.VerifiedCredential.App.DependencyInjection;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.VerifiedCredential.App;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending consents from the database
/// </summary>
public class ExpiryCheckService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExpiryCheckService> _logger;
    private readonly ExpiryCheckServiceSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="ExpiryCheckService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="options">The options</param>
    public ExpiryCheckService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpiryCheckService> logger,
        IOptions<ExpiryCheckServiceSettings> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    /// <summary>
    /// Handles the
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var processServiceScope = _serviceScopeFactory.CreateScope();
                var portalRepositories = processServiceScope.ServiceProvider.GetRequiredService<IPortalRepositories>();
                var dateTimeProvider = processServiceScope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var mailingService = processServiceScope.ServiceProvider.GetRequiredService<IMailingService>();
                var processIdentityDataDetermination = processServiceScope.ServiceProvider.GetRequiredService<IProcessIdentityDataDetermination>();
                //call processIdentityDataDetermination.GetIdentityData() once to initialize IdentityService IdentityData for synchronous use:
                await processIdentityDataDetermination.GetIdentityData().ConfigureAwait(false);

                var now = dateTimeProvider.OffsetNow;
                var companySsiDetailsRepository = portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
                var notificationRepository = portalRepositories.GetInstance<INotificationRepository>();
                var inactiveVcsToDelete = now.AddDays(-(_settings.InactiveVcsToDeleteInWeeks * 7));
                var expiredVcsToDelete = now.AddMonths(-_settings.ExpiredVcsToDeleteInMonth);
                var credentials = companySsiDetailsRepository.GetExpiryData(now, inactiveVcsToDelete, expiredVcsToDelete);
                await foreach (var credential in credentials)
                {
                    await ProcessCredentials(credential, inactiveVcsToDelete, expiredVcsToDelete, companySsiDetailsRepository, now, mailingService, notificationRepository, portalRepositories);
                }
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Verified Credential expiry check failed with: {Errors}", ex.Message);
            }
        }
    }

    private static async Task ProcessCredentials(
        CredentialExpiryData data,
        DateTimeOffset inactiveVcsToDelete,
        DateTimeOffset expiredVcsToDelete,
        ICompanySsiDetailsRepository companySsiDetailsRepository,
        DateTimeOffset now,
        IMailingService mailingService,
        INotificationRepository notificationRepository,
        IPortalRepositories portalRepositories)
    {
        switch (data.CompanySsiDetailStatusId)
        {
            case CompanySsiDetailStatusId.INACTIVE when data.DateCreated < inactiveVcsToDelete:
            case CompanySsiDetailStatusId.ACTIVE or CompanySsiDetailStatusId.INACTIVE when data.ExpiryDate < expiredVcsToDelete:
                companySsiDetailsRepository.RemoveSsiDetail(data.Id);
                break;
            case CompanySsiDetailStatusId.PENDING when data.ExpiryDate < now:
                await HandleDecline(data, mailingService, companySsiDetailsRepository, notificationRepository).ConfigureAwait(false);
                break;
            default:
                await HandleNotification(data, now, mailingService, companySsiDetailsRepository, notificationRepository).ConfigureAwait(false);
                break;
        }

        // Saving here to make sure the each credential is handled by there own 
        await portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static async ValueTask HandleDecline(CredentialExpiryData data, IMailingService mailingService, ICompanySsiDetailsRepository companySsiDetailsRepository, INotificationRepository notificationRepository)
    {
        var content = JsonSerializer.Serialize(new { Type = data.VerifiedCredentialTypeId, CredentialId = data.Id }, Options);
        notificationRepository.CreateNotification(data.UserMailingData.Id, NotificationTypeId.CREDENTIAL_REJECTED, false, n =>
        {
            n.CreatorUserId = data.UserMailingData.Id;
            n.Content = content;
        });

        companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(data.Id, c =>
            {
                c.CompanySsiDetailStatusId = data.CompanySsiDetailStatusId;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
            });

        var typeValue = data.VerifiedCredentialTypeId.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {data.VerifiedCredentialTypeId.ToString()} does not exists");
        var email = data.UserMailingData.Email;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var userName = string.Join(" ", new[] { data.UserMailingData.Firstname, data.UserMailingData.Lastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ? userName : email },
                { "requestName", typeValue },
                { "reason", "The credential is already expired" }
            };

            await mailingService.SendMails(email, mailParameters, Enumerable.Repeat("CredentialRejected", 1)).ConfigureAwait(false);
        }
    }

    private static async ValueTask HandleNotification(CredentialExpiryData data, DateTimeOffset now, IMailingService mailingService, ICompanySsiDetailsRepository companySsiDetailsRepository, INotificationRepository notificationRepository)
    {
        ExpiryCheckTypeId? newExpiryCheckTypeId = null;
        if (data.ExpiryDate.AddDays(-1) <= now && data.ExpiryCheckTypeId != ExpiryCheckTypeId.OneDay)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.OneDay;
        }

        if (data.ExpiryDate.AddDays(-14) <= now && data.ExpiryCheckTypeId != ExpiryCheckTypeId.TwoWeeks)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.TwoWeeks;
        }

        if (data.ExpiryDate.AddMonths(-1) <= now && data.ExpiryCheckTypeId == null)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.OneMonth;
        }

        if (newExpiryCheckTypeId == null)
        {
            return;
        }

        companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(
            data.Id,
            csd =>
            {
                csd.ExpiryCheckTypeId = data.ExpiryCheckTypeId;
            },
            csd =>
            {
                csd.ExpiryCheckTypeId = newExpiryCheckTypeId;
            });

        var content = JsonSerializer.Serialize(new
        {
            Type = data.VerifiedCredentialTypeId,
            ExpiryDate = data.ExpiryDate.ToString("O"),
            Version = data.DetailVersion,
            CredentialId = data.Id,
            ExpiryCheckTypeId = newExpiryCheckTypeId
        }, Options);
        notificationRepository.CreateNotification(data.UserMailingData.Id, NotificationTypeId.CREDENTIAL_EXPIRY, false, n =>
        {
            n.CreatorUserId = data.UserMailingData.Id;
            n.Content = content;
        });

        var email = data.UserMailingData.Email;
        var typeValue = data.VerifiedCredentialTypeId.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {data.VerifiedCredentialTypeId.ToString()} does not exists");
        if (!string.IsNullOrWhiteSpace(email))
        {
            var userName = string.Join(" ", new[] { data.UserMailingData.Firstname, data.UserMailingData.Lastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ? userName : email },
                { "typeId", typeValue },
                { "version", data.DetailVersion ?? "no version" },
                { "expiryDate", data.ExpiryDate.ToString("dd MMMM yyyy") }
            };

            await mailingService.SendMails(email, mailParameters, Enumerable.Repeat("CredentialExpiry", 1)).ConfigureAwait(false);
        }
    }
}
