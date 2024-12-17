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

public class ServicesServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)ServicesServiceErrors.SERVICES_NOT_EXIST, "Service {serviceId} does not exist"),
        new((int)ServicesServiceErrors.SERVICES_SUBSCRIPTION_NOT_EXIST, "Subscription {subscriptionId} does not exist")
    ]);

    public Type Type { get => typeof(ServicesServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum ServicesServiceErrors
{
    SERVICES_NOT_EXIST,
    SERVICES_SUBSCRIPTION_NOT_EXIST
}
