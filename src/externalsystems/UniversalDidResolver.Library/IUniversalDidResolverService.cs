/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library;

public interface IUniversalDidResolverService
{
    /// <summary>
    /// Validates the did using the universal resolver
    /// </summary>
    /// <param name="did">The did that should be checked</param>
    /// <param name="cancellationToken">The CancellationToken</param>
    /// <returns><c>true</c> if the did is valid, otherwise <c>false</c></returns>
    Task<DidValidationResult> ValidateDid(string did, CancellationToken cancellationToken);

    Task<bool> ValidateSchema(JsonElement content, CancellationToken cancellationToken);
}
