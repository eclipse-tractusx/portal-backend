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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class CryptoHelperTests
{
    private readonly IFixture _fixture;
    public CryptoHelperTests()
    {
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData(PaddingMode.ANSIX923)]
    [InlineData(PaddingMode.ISO10126)]
    [InlineData(PaddingMode.PKCS7)]
    public void EncryptDecrypt_ECB_NoIV_Success(PaddingMode paddingMode)
    {
        var data = _fixture.Create<string>();
        var key = _fixture.CreateMany<byte>(32).ToArray();
        var (encrypted, _) = CryptoHelper.Encrypt(data, key, CipherMode.ECB, paddingMode);
        var result = CryptoHelper.Decrypt(encrypted, null, key, CipherMode.ECB, paddingMode);
        result.Should().Be(data);
    }

    [Theory]
    [InlineData(CipherMode.CBC, PaddingMode.ANSIX923)]
    [InlineData(CipherMode.CBC, PaddingMode.ISO10126)]
    [InlineData(CipherMode.CBC, PaddingMode.PKCS7)]
    [InlineData(CipherMode.CFB, PaddingMode.ANSIX923)]
    [InlineData(CipherMode.CFB, PaddingMode.ISO10126)]
    [InlineData(CipherMode.CFB, PaddingMode.None)]
    [InlineData(CipherMode.CFB, PaddingMode.PKCS7)]
    [InlineData(CipherMode.CFB, PaddingMode.Zeros)]
    [InlineData(CipherMode.ECB, PaddingMode.ANSIX923)]
    [InlineData(CipherMode.ECB, PaddingMode.ISO10126)]
    [InlineData(CipherMode.ECB, PaddingMode.PKCS7)]

    public void EncryptDecrypt_WithIV_Success(CipherMode cipherMode, PaddingMode paddingMode)
    {
        var data = _fixture.Create<string>();
        var key = _fixture.CreateMany<byte>(32).ToArray();
        var (encrypted, iv) = CryptoHelper.Encrypt(data, key, cipherMode, paddingMode);
        var result = CryptoHelper.Decrypt(encrypted, iv, key, cipherMode, paddingMode);
        result.Should().Be(data);
    }

    [Theory]
    [InlineData("Sup3rS3cureTest!", "2b7e151628aed2a6abf715892b7e151628aed2a6abf715892b7e151628aed2a6", CipherMode.ECB, PaddingMode.PKCS7)]
    [InlineData("Sup3rS3cureTest!", "5892b7e151628aed2a6abf715892b7e151628aed2a62b7e151628aed2a6abf71", CipherMode.CBC, PaddingMode.PKCS7)]
    public void Foo(string data, string key, CipherMode cipherMode, PaddingMode paddingMode)
    {
        var (foo, bar) = CryptoHelper.Encrypt(data, Convert.FromHexString(key), cipherMode, paddingMode);
        var secret = Convert.ToBase64String(foo);
        var iv = Convert.ToBase64String(bar);                    // used to create testdata for OnboardingServiceProviderBusinessLogicTests
        secret.Should().NotBeNullOrWhiteSpace();
        iv.Should().NotBeNullOrWhiteSpace();
    }
}
