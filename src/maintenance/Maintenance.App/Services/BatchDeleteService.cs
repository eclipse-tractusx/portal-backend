/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

/// <inheritdoc />
public class BatchDeleteService(
    ILogger<BatchDeleteService> logger,
    IOptions<BatchDeleteServiceSettings> options,
    IPortalRepositories portalRepositories,
    IDateTimeProvider dateTimeProvider) : IBatchDeleteService
{
    private readonly BatchDeleteServiceSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task CleanupDocuments(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting documents and assignments older {Days} days", _settings.DeleteIntervalInDays);
            var documentRepository = portalRepositories.GetInstance<IDocumentRepository>();

            var documentData = await documentRepository
                .GetDocumentDataForCleanup(dateTimeProvider.OffsetNow.AddDays(-_settings.DeleteIntervalInDays))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (documentData.Count == 0)
            {
                logger.LogInformation("No documents to cleanup");
                return;
            }

            logger.LogInformation("Cleaning up {DocumentCount} Documents and {OfferIdCount} OfferAssignedDocuments", documentData.Count, documentData.SelectMany(x => x.OfferIds).Count());

            portalRepositories.GetInstance<IAgreementRepository>().AttachAndModifyAgreements(
                documentData.SelectMany(data => data.AgreementIds.Select<Guid, (Guid, Action<Agreement>?, Action<Agreement>)>(agreementId => (
                    agreementId,
                    agreement => agreement.DocumentId = data.DocumentId,
                    agreement => agreement.DocumentId = null))));

            portalRepositories.GetInstance<IOfferRepository>().RemoveOfferAssignedDocuments(
                documentData.SelectMany(data => data.OfferIds.Select(offerId => (
                    offerId,
                    data.DocumentId))));

            documentRepository.RemoveDocuments(documentData.Select(x => x.DocumentId));

            await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
            logger.LogInformation("Documents older than {Days} days and depending consents successfully cleaned up", _settings.DeleteIntervalInDays);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database clean up failed with error: {Errors}", ex.Message);
        }
    }
}
