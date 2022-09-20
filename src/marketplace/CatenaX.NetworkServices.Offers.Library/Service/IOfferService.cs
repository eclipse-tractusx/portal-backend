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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Offers.Library.Service;

/// <summary>
/// Business logic for handling offer-related operations. Includes persistence layer access.
/// </summary>
public interface IOfferService
{
    /// <summary>
    /// Creates new offer agreement consents with the given data for the given offer
    /// </summary>
    /// <param name="subscriptionId">Id of the offer to create the consents for.</param>
    /// <param name="agreementId">id of the agreement the consent is created for</param>
    /// <param name="consentStatusId">consent status</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the id of the created consent</returns>
    Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Updates the existing consent offer subscriptions in the database and creates new entries for the non existing.
    /// </summary>
    /// <param name="subscriptionId">Id of the offer subscription</param>
    /// <param name="offerAgreementConsentDatas">List of the agreement and status of the consent</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        IEnumerable<ServiceAgreementConsentData> offerAgreementConsentDatas,
        string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer agreement data
    /// </summary>
    /// <param name="offerId">Id of the offer to get the agreements for</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetOfferAgreement(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer consent detail data
    /// </summary>
    /// <param name="consentId">Id of the offer consent</param>
    /// <returns>Returns the details</returns>
    Task<ConsentDetailData> GetConsentDetailDataAsync(Guid consentId, OfferTypeId offerTypeId);
}
