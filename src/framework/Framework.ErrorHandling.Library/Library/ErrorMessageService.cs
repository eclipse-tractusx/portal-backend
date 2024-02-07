/********************************************************************************
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

using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;

public sealed class ErrorMessageService : IErrorMessageService
{
    private readonly IReadOnlyDictionary<Type, IReadOnlyDictionary<int, string>> _messageContainers;

    public ErrorMessageService(IEnumerable<IErrorMessageContainer> errorMessageContainers)
    {
        _messageContainers = errorMessageContainers.ToImmutableDictionary(x => x.Type, x => x.MessageContainer);
    }

    public string GetMessage(Type type, int code)
    {
        if (!_messageContainers.TryGetValue(type, out var container))
            throw new ArgumentException($"unexpected type {type.Name}");

        if (!container.TryGetValue(code, out var message))
            throw new ArgumentException($"no message defined for {type.Name}.{Enum.GetName(type, code)}");

        return message;
    }
}
