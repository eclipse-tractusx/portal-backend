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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AgreementRepository : IAgreementRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Creates a new instance of <see cref="AgreementRepository"/>
    /// </summary>
    /// <param name="context">Access to the database context</param>
    public AgreementRepository(PortalDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> CheckAgreementExistsAsync(Guid agreementId) =>
        _context.Agreements.AnyAsync(x => x.Id == agreementId);

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceAgreementData> GetServiceAgreementDataForIamUser(string iamUserId) =>
        _context.Agreements
            .Where(x => x.AgreementAssignedOffers
                .Any(app => 
                    app.Offer!.OfferTypeId == OfferTypeId.SERVICE &&
                    (app.Offer!.OfferSubscriptions.Any(os => os.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)) ||
                    app.Offer!.ProviderCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId))
                ))
            .Select(x => new ServiceAgreementData(x.Id, x.Name))
            .AsAsyncEnumerable();
}
