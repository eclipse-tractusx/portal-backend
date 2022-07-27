/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// model for Language.
/// </summary>
public class LanguageData
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="languageShortName">Language Short Name</param>
    /// <param name="longNameDe">Language Long Name in German</param>
    /// <param name="longNameEn">Language Long Name in English</param>
    public LanguageData(string languageShortName, string longNameDe, string longNameEn)
    {
        ShortName = languageShortName;
        LongNameDe = longNameDe;
        LongNameEn = longNameEn;
    }
    
    /// <summary>
    /// Language Short Name
    /// </summary>
    /// <value>languageShortName</value>
    [JsonPropertyName("languageShortName")]
    public string ShortName { get; private set; }
    
    /// <summary>
    /// Language Long Name in German
    /// </summary>
    /// <value>languageShortNameDe</value>
    [JsonPropertyName("languageShortNameDe")]
    public string LongNameDe { get; set; }
    
    /// <summary>
    /// Language Long Name in English
    /// </summary>
    /// <value>languageShortNameEn</value>
    [JsonPropertyName("languageShortNameEn")]
    public string LongNameEn { get; set; }
}