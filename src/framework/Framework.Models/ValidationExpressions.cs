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
}
