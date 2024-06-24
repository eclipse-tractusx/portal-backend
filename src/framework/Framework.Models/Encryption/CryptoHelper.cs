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
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;

public class CryptoHelper
{
    private readonly byte[] _encryptionKey;

    private readonly CipherMode _cipherMode;
    private readonly PaddingMode _paddingMode;

    public CryptoHelper(byte[] encryptionKey, CipherMode cipherMode, PaddingMode paddingMode)
    {
        _encryptionKey = encryptionKey;
        _cipherMode = cipherMode;
        _paddingMode = paddingMode;
    }

    public (byte[] Result, byte[] InitializationVector) Encrypt(string data)
    {
        try
        {
            return Encrypt(data, _encryptionKey, _cipherMode, _paddingMode);
        }
        catch (ArgumentException e)
        {
            throw new ConfigurationException($"Invalid Encryption Config: {e.Message}", e);
        }
        catch (CryptographicException e)
        {
            throw new ConflictException($"Data could not be encrypted: {e.Message}", e);
        }
    }

    public string Decrypt(byte[] data, byte[]? initializationVector)
    {
        try
        {
            return Decrypt(data, initializationVector, _encryptionKey, _cipherMode, _paddingMode);
        }
        catch (ArgumentException e)
        {
            throw new ConfigurationException($"Invalid Encryption Config: {e.Message}", e);
        }
        catch (CryptographicException e)
        {
            throw new ConflictException($"Data could not be decrypted: {e.Message}", e);
        }
    }

    public static (byte[] Result, byte[] InitializationVector) Encrypt(string data, byte[] encryptionKey, CipherMode cipherMode, PaddingMode paddingMode)
    {
        using var aes = Aes.Create();
        aes.Mode = cipherMode;
        aes.Padding = paddingMode;
        var encryptor = aes.CreateEncryptor(encryptionKey, aes.IV);
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            using var sw = new StreamWriter(cryptoStream, Encoding.UTF8);
            sw.Write(data);
        }
        return (memoryStream.ToArray(), aes.IV);
    }

    public static string Decrypt(byte[] data, byte[]? initializationVector, byte[] encryptionKey, CipherMode cipherMode, PaddingMode paddingMode)
    {
        using var aes = Aes.Create();
        aes.Mode = cipherMode;
        aes.Padding = paddingMode;
        aes.Key = encryptionKey;
        if (initializationVector != null)
        {
            aes.IV = initializationVector;
        }
        var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(data);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);
        return srDecrypt.ReadToEnd();
    }
}
