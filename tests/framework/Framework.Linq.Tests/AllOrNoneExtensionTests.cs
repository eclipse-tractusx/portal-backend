/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq.Tests;

public class AllOrNoneExtensionTests
{
    #region AllOrNone

    [Theory]
    [InlineData(new bool[] { }, true)]
    [InlineData(new[] { true }, true)]
    [InlineData(new[] { false }, true)]
    [InlineData(new[] { false, false, false }, true)]
    [InlineData(new[] { false, false, true }, false)]
    [InlineData(new[] { false, true, false }, false)]
    [InlineData(new[] { false, true, true }, false)]
    [InlineData(new[] { true, false, false }, false)]
    [InlineData(new[] { true, false, true }, false)]
    [InlineData(new[] { true, true, true }, true)]
    public void AllOrNone_ReturnsExpected(IEnumerable<bool> items, bool expected)
    {
        // Act
        var result = items.AllOrNone();

        // Assert result
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(new string[] { }, true)]
    [InlineData(new[] { "true" }, true)]
    [InlineData(new[] { "false" }, true)]
    [InlineData(new[] { "false", "false", "false" }, true)]
    [InlineData(new[] { "false", "false", "true" }, false)]
    [InlineData(new[] { "false", "true", "false" }, false)]
    [InlineData(new[] { "false", "true", "true" }, false)]
    [InlineData(new[] { "true", "false", "false" }, false)]
    [InlineData(new[] { "true", "false", "true" }, false)]
    [InlineData(new[] { "true", "true", "true" }, true)]
    public void AllOrNone_WithPredicate_ReturnsExpected(IEnumerable<string> items, bool expected)
    {
        // Act
        var result = items.AllOrNone(bool.Parse);

        // Assert result
        result.Should().Be(expected);
    }

    #endregion
}
