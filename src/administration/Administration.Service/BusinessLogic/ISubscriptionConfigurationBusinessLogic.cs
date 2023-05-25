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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface ISubscriptionConfigurationBusinessLogic
{
    /// <summary>
    /// Retriggers the process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    Task RetriggerProvider(Guid offerSubscriptionId);

    /// <summary>
    /// Retriggers the process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    Task RetriggerCreateClient(Guid offerSubscriptionId);

    /// <summary>
    /// Retriggers the process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    Task RetriggerCreateTechnicalUser(Guid offerSubscriptionId);

    /// <summary>
    /// Retriggers the process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    Task RetriggerProviderCallback(Guid offerSubscriptionId);

    /// <summary>
    /// Gets the process steps for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    /// <returns>Returns the process steps with their status</returns>
    IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId);

    /// <summary>
    /// Gets the service provider company details
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>The detail data</returns>
    Task<ProviderDetailReturnData> GetProviderCompanyDetailsAsync(string iamUserId);

    /// <summary>
    /// Sets service provider company details
    /// </summary>
    /// <param name="data">Detail data for the service provider</param>
    /// <param name="iamUserId">Id of the iam user</param>
    Task SetProviderCompanyDetailsAsync(ProviderDetailData data, string iamUserId);
}
