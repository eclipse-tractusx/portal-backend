/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Models;

public class CompanyInvitationDataTests
{
    [Theory]
    [InlineData("postmaster@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]")]
    [InlineData("postmaster@[123.123.123.123]")]
    [InlineData("user-@example.org")]
    [InlineData("user%example.com@example.org")]
    [InlineData("mailhost!username@example.org")]
    [InlineData("\"john..doe\"@example.org")]
    [InlineData("\" \"@example.org")]
    [InlineData("example@s.example")]
    [InlineData("admin@example")]
    [InlineData("name/surname@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("long.email-address-with-hyphens@and.subdomains.example.com")]
    [InlineData("x@example.com")]
    [InlineData("very.common@example.com")]
    [InlineData("simple@example.com")]
    public void ValidateEmail_WithValidEmail_ReturnsExpected(string email)
    {
        // Arrange
        var sut = new CompanyInvitationData("fixed", "fixed", "fixed", email, "fixed");

        // Act
        var result = Validator.TryValidateObject(sut, new ValidationContext(sut, null, null), null, true);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc.example.com")]
    [InlineData("a@b@c@example.com")]
    public void ValidateEmail_WithInvalidEmail_ReturnsExpected(string email)
    {
        // Arrange
        var sut = new CompanyInvitationData("fixed", "fixed", "fixed", email, "fixed");

        // Act
        var result = Validator.TryValidateObject(sut, new ValidationContext(sut, null, null), null, true);

        // Assert
        result.Should().BeFalse();
    }
}
