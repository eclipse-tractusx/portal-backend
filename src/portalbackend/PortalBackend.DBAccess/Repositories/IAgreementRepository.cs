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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing agreements on the persistence layer.
/// </summary>
public interface IAgreementRepository
{
    /// <summary>
    /// Checks whether the agreement with the given id exists. 
    /// </summary>
    /// <param name="agreementId">Id of the agreement</param>
    /// <param name="subscriptionId">Id of subscription the agreement must be associated with</param>
    /// <param name="offerTypeId">The OfferTypeId that the agreement must be associated with</param>
    /// <returns>Returns <c>true</c> if an agreement was found, otherwise <c>false</c>.</returns>
    Task<bool> CheckAgreementExistsForSubscriptionAsync(Guid agreementId, Guid subscriptionId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the agreement data that have an app id set
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="offerTypeId">Specific offer type</param>
    /// <returns>Returns an async enumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetOfferAgreementDataForOfferId(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the agreement data untracked from the database
    /// </summary>
    /// <returns>Returns an async enumerable of agreement data</returns>
    IAsyncEnumerable<AgreementDocumentData> GetAgreementsForCompanyRolesUntrackedAsync();

    /// <summary>
    /// Return all agreements for agreement category app_contract
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetAgreementDataForOfferType(OfferTypeId offerTypeId);

    /// <summary>
    /// Return matching Agreement and Consent for agreement category app_contract and offer id
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userCompanyId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(OfferAgreementConsent OfferAgreementConsent, bool IsProviderCompany)> GetOfferAgreementConsentById(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId);

    /// <summary>
    /// Return matching Agreement ,Consent,CompanyUserId and CompanyId for agreement category app_contract , offer id and offer status created
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userCompanyId"></param>
    /// <param name="statusId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(OfferAgreementConsentUpdate OfferAgreementConsentUpdate, bool IsProviderCompany)> GetOfferAgreementConsent(Guid appId, Guid userCompanyId, OfferStatusId statusId, OfferTypeId offerTypeId);

    /// <summary>
    /// Checks whether the given agreements exists in the database
    /// </summary>
    /// <param name="agreementIds">Ids of the agreements</param>
    /// <param name="subscriptionId">Id of subscription the agreement must be associated with</param>
    /// <param name="offerTypeId">The OfferTypeId that the agreement must be associated with</param>
    /// <returns>Returns <c>true</c> if the agreements were found, otherwise <c>false</c>.</returns>
    Task<bool> CheckAgreementsExistsForSubscriptionAsync(IEnumerable<Guid> agreementIds, Guid subscriptionId, OfferTypeId offerTypeId);

    /// <summary>
    /// Returns all agreeementIds associated with a given offer
    /// </summary>
    /// <param name="offerId">Id of the offer the agreement must be associated with</param>
    /// <returns></returns>
    IAsyncEnumerable<Guid> GetAgreementIdsForOfferAsync(Guid offerId);
}
