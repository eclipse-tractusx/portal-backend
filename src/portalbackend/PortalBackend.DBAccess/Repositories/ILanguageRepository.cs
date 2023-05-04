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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ILanguageRepository
{
    /// <summary>
    /// Gets the languages with the matching short name
    /// </summary>
    /// <param name="languageShortName">the short name of the language</param>
    /// <returns>true if existing otherwise false</returns>
    Task<bool> IsValidLanguageCode(string languageShortName);

    /// <summary>
    /// Checks whether the given language codes exists in the persistence storage
    /// </summary>
    /// <param name="languageCodes">the language codes that should be checked</param>
    /// <returns>Returns the found language codes</returns>
    IAsyncEnumerable<string> GetLanguageCodesUntrackedAsync(IEnumerable<string> languageCodes);
}
