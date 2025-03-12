/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public static class ValidationExpressions
{
    public const string Name = @"^.+$";
    public const string Bpn = @"^(BPNL|bpnl)[\w|\d]{12}$";
    public const string Bpns = @"^(BPNS|bpns)[\w|\d]{12}$";
    /// <summary>
    /// Regular expression pattern for validating legal company names.
    /// </summary>
    /// <remarks>
    /// The pattern ensures the following:
    /// - unicode category \p{L} for letters, \u0E00-\u0E7F for Thai characters
    /// - digits, currency symbols, and various special characters.
    /// - The string can have spaces between characters but not at the end.
    /// - The length of the string must be between 1 and 160 characters.
    /// </remarks>
    public const string Company = @"^(?!.*\s$)([\p{L}\u0E00-\u0E7F\d\p{Sc}@%*+_\-/\\,.:;=<>!?&^#'\x22()[\]]\s?){1,160}$";
    public const string ExternalCertificateNumber = @"^[a-zA-Z0-9]{0,36}$";
    /// <summary>
    /// To validate Region field of Address.
    /// </summary>
    /// <remarks>
    /// The pattern ensures ISO-1366-2 code value: NW
    /// </remarks>
    public const string Region = "^[A-Z1-9]{1,3}$";

    #region UniqueIdentifiers
    public static readonly IReadOnlyDictionary<string, string> COMMERCIAL_REG_NUMBER = new Dictionary<string, string>
        {
            { "Worldwide", "^(?!.*\\s$)([A-Za-zÀ-ÿ0-9.()](\\.|\\s|-|_)?){4,50}$" },
            { "DE", "^(?!.*\\s$)([A-Za-zÀ-ÿ])([A-Za-zÀ-ÿ0-9.()](\\s|-|_)?){4,50}$" },
            { "FR", "^(?!.*\\s$)([A-Za-z0-9]\\s?){14,17}$" },
        }.ToImmutableDictionary();
    public static readonly IReadOnlyDictionary<string, string> VAT_ID = new Dictionary<string, string>
        {
            { "Worldwide", "^(?!.*\\s$)([A-Za-z0-9](\\.|\\s|-|\\/)?){5,18}$" },
            { "DE", "^DE\\d{9}$" },
            { "IN", "^[a-zA-Z\\d-]{5,15}$" },
            { "MX", "^[a-zA-Z\\d-&]{12,13}$" },
        }.ToImmutableDictionary();
    public static readonly IReadOnlyDictionary<string, string> VIES = new Dictionary<string, string>
        {
            { "Worldwide", "^[A-Z]{2}[0-9A-Za-z+*.]{2,12}$" }
        }.ToImmutableDictionary();
    public static readonly IReadOnlyDictionary<string, string> EORI = new Dictionary<string, string>
        {
            { "Worldwide", "^[A-Z]{2}[A-Za-z0-9]{1,15}$" }
        }.ToImmutableDictionary();
    public static readonly IReadOnlyDictionary<string, string> LEI_CODE = new Dictionary<string, string>
        {
            { "Worldwide", "^[A-Za-z0-9]{20}$" }
        }.ToImmutableDictionary();
    #endregion
}
