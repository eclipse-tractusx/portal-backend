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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;

public class EncryptionModeConfig
{
    [Required]
    public int Index { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string EncryptionKey { get; set; } = null!;

    [Required]
    [ValidateEnumValue]
    public CipherMode CipherMode { get; set; }

    [Required]
    [ValidateEnumValue]
    public PaddingMode PaddingMode { get; set; }
}

public static class EncryptionModeConfigExtension
{
    public static CryptoHelper GetCryptoHelper(this IEnumerable<EncryptionModeConfig> configs, int index)
    {
        var cryptoConfig = configs.SingleOrDefault(x => x.Index == index) ?? throw new ConfigurationException($"EncryptionModeIndex {index} is not configured");
        try
        {
            return new(Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        }
        catch (FormatException)
        {
            throw new ConfigurationException($"EncryptionModeConfig index {index} is not valid. EncryptionKey cannot be parsed as hex-string");
        }
    }
}
