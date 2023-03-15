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
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Business logic for handling service release-related operations. Includes persistence layer access.
/// </summary>
public interface IServiceReleaseBusinessLogic
{
    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync();

    /// <summary>
    /// Retrieve Service Details by Id
    /// </summary>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    Task<ServiceData> GetServiceDetailsByIdAsync(Guid serviceId);
    
    /// <summary>
    /// Retrieve Service Type Data
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync();

    /// <summary>
    /// Retrieve Offer Agreemnet Consent Status Data
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetServiceAgreementConsentAsync(Guid serviceId, string iamUserId);
}
