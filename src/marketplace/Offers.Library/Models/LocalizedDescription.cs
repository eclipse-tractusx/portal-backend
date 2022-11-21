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

using System.ComponentModel.DataAnnotations;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;

/// <summary>
/// Simple model to specify descriptions for a language.
/// </summary>
public class LocalizedDescription
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="languageCode">Two character language code.</param>
    /// <param name="longDescription">Long description in specified language.</param>
    /// <param name="shortDescription">Short description in specified language.</param>
    public LocalizedDescription(string languageCode, string longDescription, string shortDescription)
    {
        LanguageCode = languageCode;
        LongDescription = longDescription;
        ShortDescription = shortDescription;
    }

    /// <summary>
    /// Two character language code.
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string LanguageCode { get; set; }

    /// <summary>
    /// Long description in specified language.
    /// </summary>
    [MaxLength(4096)]
    public string LongDescription { get; set; }

    /// <summary>
    /// Short description in specified language.
    /// </summary>
    [MaxLength(255)]
    public string ShortDescription { get; set; }
}

/// <summary>
/// Model for LanguageCode and Description
/// </summary>
/// <param name="LanguageCode"></param>
/// <param name="LongDescription"></param>
/// <param name="ShortDescription"></param>
/// <returns></returns>
public record Localization(string LanguageCode, string LongDescription, string ShortDescription);
