﻿/********************************************************************************
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

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
    Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields);
    
    /// <summary>
    /// Attaches the consents to the database
    /// </summary>
    /// <param name="consents">The consents that should be attached to the database.</param>
    void AttachToDatabase(IEnumerable<Consent> consents);

    /// <summary>
    /// Remove the given consents from the database
    /// </summary>
    /// <param name="consents">The consents that should be removed.</param>
    void RemoveConsents(IEnumerable<Consent> consents);
}