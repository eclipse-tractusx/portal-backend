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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing consents on the persistence layer.
/// </summary>
public interface IConsentRepository
{
    /// <summary>
    /// Creates a consent with the given data in the database.
    /// </summary>
    /// <param name="agreementId">Id of the agreement</param>
    /// <param name="companyId">Id of the company</param>
    /// <param name="companyUserId">Id of the company User</param>
    /// <param name="consentStatusId">Id of the consent status</param>
    /// <param name="setupOptionalFields">Action to setup the optional fields of the consent</param>
    /// <returns>Returns the newly created consent</returns>
    Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields = null);
    
    /// <summary>
    /// Remove the given consents from the database
    /// </summary>
    /// <param name="consents">The consents that should be removed.</param>
    void RemoveConsents(IEnumerable<Consent> consents);

    /// <summary>
    /// Add consent Id and offer Id in consent_assigned_offer table
    /// </summary>
    /// <param name="consentId"></param>
    /// <param name="offerId"></param>
    /// <returns></returns>
    ConsentAssignedOffer CreateConsentAssignedOffer(Guid consentId, Guid offerId);

    /// <summary>
    /// Gets the details of the consent
    /// </summary>
    /// <param name="consentId">Id of the Consent</param>
    /// <param name="offerTypeId">OfferTypeId the consent must be assiciated with</param>
    /// <returns>Returns the detail data of the consent</returns>
    Task<ConsentDetailData?> GetConsentDetailData(Guid consentId, OfferTypeId offerTypeId);

    /// <summary>
    /// Updates the given consents
    /// </summary>
    /// <param name="consentIds">Collection of consets that should be updated</param>
    /// <param name="setOptionalParameter">Action that will be applied to all consents</param>
    void AttachAndModifiesConsents(IEnumerable<Guid> consentIds, Action<Consent> setOptionalParameter);
}
