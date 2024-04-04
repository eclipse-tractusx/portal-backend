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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.IO.Tests;

public class UrlHelperTests
{
    [Theory]
    [InlineData("http://www.test.de")]
    [InlineData("http://www.test.de/")]
    [InlineData("https://www.test.de")]
    [InlineData("https://www.test.de/")]
    public void EnsureValidHttpUrl_WithValidUrl_NoErrorThrown(string url)
    {
        // Act
        url.EnsureValidHttpUrl(() => nameof(url));

        // Assert
        url.Should().Be(url); // One assert is needed
    }

    [Fact]
    public void EnsureValidHttpUrl_WithNotValidUri_ThrowsControllerArgumentException()
    {
        // Arrange
        const string url = "123";

        // Act
        void Act() => url.EnsureValidHttpUrl(() => "test");

        // Assert
        var ex = Assert.Throws<ControllerArgumentException>(Act);
        ex.Message.Should().Contain($"url {url} cannot be parsed: Invalid URI: The format of the URI could not be determined. (Parameter 'test')");
    }

    [Theory]
    [InlineData("ftp://www.test.de")]
    [InlineData("c:/path/to/glory")]
    [InlineData("//test.com")]
    public void EnsureValidHttpUrl_WithInvalidScheme_ThrowsControllerArgumentException(string url)
    {
        // Act
        void Act() => url.EnsureValidHttpUrl(() => nameof(url));

        // Assert
        var ex = Assert.Throws<ControllerArgumentException>(Act);
        ex.Message.Should().Contain($"url {url} must either start with http:// or https://");
    }

    [Fact]
    public void EnsureValidHttpUrl_WithNotWellFormattedUri_ThrowsControllerArgumentException()
    {
        // Arrange
        const string url = "http://www.test.com/path???/file name";

        // Act
        void Act() => url.EnsureValidHttpUrl(() => "test");

        // Assert
        var ex = Assert.Throws<ControllerArgumentException>(Act);
        ex.Message.Should().Contain($"url {url} is not wellformed (Parameter 'test')");
    }
}
