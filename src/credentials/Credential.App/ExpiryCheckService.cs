/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Credential.App.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Credential.App;

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
            var credentials = await companySsiDetailsRepository.GetExpiryData(now, inactiveVcsToDelete, expiredVcsToDelete).ToListAsync(stoppingToken).ConfigureAwait(false);
            foreach (var credential in credentials)
            {
                await ProcessCredentials(credential, companySsiDetailsRepository, mailingService, notificationRepository, portalRepositories);
            }
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            _logger.LogError("Verified Credential expiry check failed with: {Errors}", ex.Message);
        }
    }

    private static async Task ProcessCredentials(
        CredentialExpiryData data,
        ICompanySsiDetailsRepository companySsiDetailsRepository,
        IMailingService mailingService,
        INotificationRepository notificationRepository,
        IPortalRepositories portalRepositories)
    {
        if (data.ScheduleData.IsVcToDelete)
        {
            companySsiDetailsRepository.RemoveSsiDetail(data.Id);
        }
        else if (data.ScheduleData.IsVcToDecline)
        {
            await HandleDecline(data, mailingService, companySsiDetailsRepository, notificationRepository).ConfigureAwait(false);
        }
        else
        {
            await HandleNotification(data, mailingService, companySsiDetailsRepository, notificationRepository).ConfigureAwait(false);
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
                { "userName", string.IsNullOrWhiteSpace(userName) ? email : userName },
                { "requestName", typeValue },
                { "reason", "The credential is already expired" }
            };

            await mailingService.SendMails(email, mailParameters, Enumerable.Repeat("CredentialRejected", 1)).ConfigureAwait(false);
        }
    }

    private static async ValueTask HandleNotification(CredentialExpiryData data, IMailingService mailingService, ICompanySsiDetailsRepository companySsiDetailsRepository, INotificationRepository notificationRepository)
    {
        ExpiryCheckTypeId? newExpiryCheckTypeId;
        if (data.ScheduleData.IsOneDayNotification)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.ONE_DAY;
        }
        else if (data.ScheduleData.IsTwoWeeksNotification)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.TWO_WEEKS;
        }
        else if (data.ScheduleData.IsOneMonthNotification)
        {
            newExpiryCheckTypeId = ExpiryCheckTypeId.ONE_MONTH;
        }
        else
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
            ExpiryDate = data.ExpiryDate?.ToString("O") ?? throw new ConflictException("Expiry Date must be set here"),
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
                { "userName", string.IsNullOrWhiteSpace(userName) ? email : userName },
                { "typeId", typeValue },
                { "version", data.DetailVersion ?? "no version" },
                { "expiryDate", data.ExpiryDate?.ToString("dd MMMM yyyy") ?? throw new ConflictException("Expiry Date must be set here") }
            };

            await mailingService.SendMails(email, mailParameters, Enumerable.Repeat("CredentialExpiry", 1)).ConfigureAwait(false);
        }
    }
}
