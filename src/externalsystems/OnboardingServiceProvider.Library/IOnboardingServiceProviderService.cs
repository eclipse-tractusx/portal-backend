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
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;

/// <summary>
/// Service for onboarding service provider related topics
/// </summary>
public interface IOnboardingServiceProviderService
{
    /// <summary>
    /// Posts the status of an application to the onboarding service provider
    /// </summary>
    /// <param name="ospDetails">Onboarding Service Provider details</param>
    /// <param name="callbackData">Data for the callback</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <exception cref="ServiceException"></exception>
    Task<bool> TriggerProviderCallback(OspTriggerDetails ospDetails, OnboardingServiceProviderCallbackData callbackData, CancellationToken cancellationToken);
}
