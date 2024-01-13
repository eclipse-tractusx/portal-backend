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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library;

/// <summary>
/// Service to handle communication with the offer providers
/// </summary>
public interface IOfferProviderService
{
    /// <summary>
    /// Triggers the offer provider
    /// </summary>
    /// <param name="autoSetupData">data needed for the call</param>
    /// <param name="autoSetupUrl">url of the provider</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="ServiceException">throws an exception if the service call wasn't successfully</exception>
    Task<bool> TriggerOfferProvider(OfferThirdPartyAutoSetupData autoSetupData, string autoSetupUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers the offer provider callback after the auto setup
    /// </summary>
    /// <param name="callbackData">the client and technical user data</param>
    /// <param name="callbackUrl">callback url of the provider</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="ServiceException">throws an exception if the service call wasn't successfully</exception>
    Task<bool> TriggerOfferProviderCallback(OfferProviderCallbackData callbackData, string callbackUrl, CancellationToken cancellationToken);
}
