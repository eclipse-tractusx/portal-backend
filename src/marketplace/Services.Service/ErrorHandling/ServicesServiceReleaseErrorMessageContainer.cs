/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.ErrorHandling;

public class ServicesServiceReleaseErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)ServicesServiceReleaseErrors.SERVICES_NOT_SERVICEID_NOT_FOUND_OR_INCORR, "serviceId {serviceId} not found or Incorrect Status"),
        new((int)ServicesServiceReleaseErrors.SERVICES_NOT_SERVICEID_NOT_FOUND_OR_INCORR, "serviceId {serviceId} not found or Incorrect Status"),
        new((int)ServicesServiceReleaseErrors.SERVICES_SERVICE_TYPE_IDS_NEVER_NULL, "serviceTypeIds should never be null here"),
        new((int)ServicesServiceReleaseErrors.SERVICES_ARGUMENT_NOT_EMPTY, "ServiceId must not be empty"),
        new((int)ServicesServiceReleaseErrors.SERVICES_NOT_EXIST, "Service {serviceId} does not exists"),
        new((int)ServicesServiceReleaseErrors.SERVICES_CONFLICT_STATE_NOT_UPDATED, "Service in State {offerState} can't be updated"),
        new((int)ServicesServiceReleaseErrors.SERVICES_FORBIDDEN_COMPANY_NOT_SERVICE_PROVIDER, "Company {companyId} is not the service provider.")
    ]);

    public Type Type { get => typeof(ServicesServiceReleaseErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum ServicesServiceReleaseErrors
{
    SERVICES_NOT_SERVICEID_NOT_FOUND_OR_INCORR,
    SERVICES_SERVICE_TYPE_IDS_NEVER_NULL,
    SERVICES_ARGUMENT_NOT_EMPTY,
    SERVICES_NOT_EXIST,
    SERVICES_CONFLICT_STATE_NOT_UPDATED,
    SERVICES_FORBIDDEN_COMPANY_NOT_SERVICE_PROVIDER
}
