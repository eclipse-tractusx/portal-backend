/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending consents from the database
/// </summary>
public class BatchDeleteService : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BatchDeleteService> _logger;
    private readonly int _days;

    /// <summary>
    /// Creates a new instance of <see cref="BatchDeleteService"/>
    /// </summary>
    /// <param name="applicationLifetime">Application lifetime</param>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="config">the apps configuration</param>
    public BatchDeleteService(
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BatchDeleteService> logger,
        IConfiguration config)
    {
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _days = config.GetValue<int>("DeleteIntervalInDays");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Getting documents and assignments older {Days} days", _days);
                List<(Guid DocumentId, IEnumerable<Guid> AgreementIds, IEnumerable<Guid> OfferIds)> documentData = await dbContext.Documents.Where(x =>
                    x.DateCreated < DateTimeOffset.UtcNow.AddDays(-_days) &&
                    x.DocumentStatusId == DocumentStatusId.INACTIVE)
                    .Select(doc => new ValueTuple<Guid, IEnumerable<Guid>, IEnumerable<Guid>>(
                        doc.Id,
                        doc.Agreements.Select(x => x.Id),
                        doc.Offers.Select(x => x.Id)
                        ))
                    .ToListAsync(stoppingToken)
                    .ConfigureAwait(false);
                _logger.LogInformation("Cleaning up {DocumentCount} Documents, {AgreementIdsCount} AgreementAssignedDocuments and {OfferIdCount} OfferAssignedDocuments", documentData.Count, documentData.SelectMany(x => x.AgreementIds).Count(), documentData.SelectMany(x => x.OfferIds).Count());

                dbContext.AgreementAssignedDocuments.RemoveRange(documentData.SelectMany(data => data.AgreementIds.Select(agreementId => new AgreementAssignedDocument(agreementId, data.DocumentId))));
                dbContext.OfferAssignedDocuments.RemoveRange(documentData.SelectMany(data => data.OfferIds.Select(offerId => new OfferAssignedDocument(offerId, data.DocumentId))));
                dbContext.Documents.RemoveRange(documentData.Select(x => new Document(x.DocumentId, null!, null!, null!, default, default, default)));
                await dbContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Documents older than {Days} days and depending consents successfully cleaned up", _days);
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Database clean up failed with error: {Errors}", ex.Message);
            }
        }

        _applicationLifetime.StopApplication();
    }
}
