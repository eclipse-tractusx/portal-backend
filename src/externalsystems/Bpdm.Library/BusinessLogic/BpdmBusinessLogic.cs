/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;

public class BpdmBusinessLogic : IBpdmBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;

    public BpdmBusinessLogic(IPortalRepositories portalRepositories, IBpdmService bpdmService)
    {
        _portalRepositories = portalRepositories;
        _bpdmService = bpdmService;
    }

    public async Task<(Action<ApplicationChecklistEntry>?, IEnumerable<ProcessStepTypeId>?, bool)> PushLegalEntity(IChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetBpdmDataForApplicationAsync(context.ApplicationId).ConfigureAwait(false);

        if (result == default)
        {
            throw new NotFoundException($"Application {context.ApplicationId} does not exists.");
        }

        if (result.BpdmData == null)
        {
            throw new UnexpectedConditionException($"BpdmData should never be null here");
        }

        var data = result.BpdmData;
        if (!string.IsNullOrWhiteSpace(data.BusinessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber is already set");
        }

        if (string.IsNullOrWhiteSpace(data.Alpha2Code))
        {
            throw new ConflictException("Alpha2Code must not be empty");
        }

        if (string.IsNullOrWhiteSpace(data.City))
        {
            throw new ConflictException("City must not be empty");
        }

        if (string.IsNullOrWhiteSpace(data.StreetName))
        {
            throw new ConflictException("StreetName must not be empty");
        }

        var bpdmTransferData = new BpdmTransferData(
            context.ApplicationId.ToString(),
            data.CompanyName,
            data.ShortName,
            data.Alpha2Code,
            data.ZipCode,
            data.City,
            data.StreetName,
            data.StreetNumber,
            data.Region,
            data.Identifiers);

        await _bpdmService.PutInputLegalEntity(bpdmTransferData, cancellationToken).ConfigureAwait(false);

        return (
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE,
            new [] { ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL },
            true);
    }

    public async Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)> HandlePullLegalEntity(IChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetBpdmDataForApplicationAsync(context.ApplicationId).ConfigureAwait(false);
        
        if (result == default)
        {
            throw new UnexpectedConditionException($"CompanyApplication {context.ApplicationId} does not exist");
        }

        var (companyId, data) = result;

        var legalEntity = await _bpdmService.FetchInputLegalEntity(context.ApplicationId.ToString(), cancellationToken).ConfigureAwait(false);

        if (legalEntity == null)
        {
            throw new ConflictException($"legal-entity not found in bpdm for application {context.ApplicationId}");
        }

        if (string.IsNullOrEmpty(legalEntity.Bpn))
        {
            return (null,null,false);
        }

        // TODO: clarify whether it should be an error if businessPartnerNumber has been set locally while bpdm-answer was outstanding
        // TODO: clarify whether it should be an error if address- or identifier-data returned by bpdm does not match what is stored in portal-db or modify in portal-db based on bpdm-response

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(
            companyId,
            company => 
            {
                company.BusinessPartnerNumber = data.BusinessPartnerNumber;
            },
            company =>
            {
                company.BusinessPartnerNumber = legalEntity.Bpn;
            });

        var registrationValidationFailed = context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == ApplicationChecklistEntryStatusId.FAILED;

        return (
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE,
            registrationValidationFailed
                ? null
                : new [] { ProcessStepTypeId.CREATE_IDENTITY_WALLET },
            true);
    }
}
