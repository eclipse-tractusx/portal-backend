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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;

public class OnboardingServiceProviderBusinessLogic : IOnboardingServiceProviderBusinessLogic
{
    private readonly IOnboardingServiceProviderService _onboardingServiceProviderService;

    public OnboardingServiceProviderBusinessLogic(IOnboardingServiceProviderService onboardingServiceProviderService)
    {
        _onboardingServiceProviderService = onboardingServiceProviderService;
    }
    
    public async Task TriggerProviderCallback(string? callbackUrl, string? bpn, Guid? externalId, Guid applicationId, string comment, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(callbackUrl))
        {
            if (externalId == null)
            {
                throw new UnexpectedConditionException("No external registration found");
            }

            if (string.IsNullOrWhiteSpace(bpn))
            {
                throw new UnexpectedConditionException("Bpn must be set");
            }

            await _onboardingServiceProviderService.TriggerProviderCallback(callbackUrl,
                    new OnboardingServiceProviderCallbackData(externalId.Value, applicationId, bpn, CompanyApplicationStatusId.DECLINED, comment),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
