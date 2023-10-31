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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging.MaskingOperator;
using Serilog.Enrichers.Sensitive;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Logging.Tests;

public class SecretOperatorTests
{
    [Theory]
    [InlineData("foobarsecret=1234&deadbeef", "foobarsecret=****&deadbeef", true)]
    [InlineData("foobarpassword=1234&deadbeef", "foobarpassword=****&deadbeef", true)]
    [InlineData("foobarSecret=1234&deadbeefPassword=5678&", "foobarSecret=****&deadbeefPassword=****&", true)]
    [InlineData("foobarpasssword=1234&deadbeef", null, false)]
    public void Mask_ReturnsExpected(string input, string matchResult, bool match)
    {
        //Arrange
        var sut = new SecretOperator();

        //Act
        var result = sut.Mask(input, "****");

        //Assert
        result.Should().Match<MaskingResult>(x =>
            x.Match == match &&
            x.Result == matchResult
        );
    }
}
