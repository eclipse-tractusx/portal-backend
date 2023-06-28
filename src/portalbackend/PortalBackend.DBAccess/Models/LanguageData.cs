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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Model for LanguageData
/// </summary>
/// <param name="LanguageShortName">Language Short Name</param>
/// <param name="LanguageLongNames">Language Long Name</param>
/// <returns></returns>
public record LanguageData(string LanguageShortName, IEnumerable<LanguageDataLongName> LanguageLongNames);

/// <summary>
/// Model for LanguageDataLongNames
/// </summary>
/// <param name="Language">language</param>
/// <param name="LongDescription">long Description</param>
/// <returns></returns>
public record LanguageDataLongName(string Language, string LongDescription);
