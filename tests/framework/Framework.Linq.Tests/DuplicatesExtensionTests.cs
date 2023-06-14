/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq.Tests;

public class DuplicatesExtensionTests
{
    private readonly IFixture _fixture;

    public DuplicatesExtensionTests()
    {
        _fixture = new Fixture();
    }

    #region Duplicates

    [Theory]
    [InlineData(new[] { "foo", "bar", "baz" }, new string[] { })]
    [InlineData(new[] { "foo", "bar", "baz", "bar", "foo" }, new[] { "bar", "foo" })]
    public void Duplicates_ReturnsExpected(IEnumerable<string> source, IEnumerable<string> expected)
    {
        // Act
        var result = source.Duplicates();

        // Assert
        result.Should().ContainInOrder(expected);
    }

    #endregion

    #region DuplicatesBy

    [Theory]
    [InlineData(new[] { "foo", "bar", "baz" }, new[] { "value1", "value2", "value3" }, new string[] { }, new string[] { })]
    [InlineData(new[] { "foo", "bar", "baz", "bar", "foo" }, new[] { "value1", "value2", "value3", "value4", "value5" }, new[] { "bar", "foo" }, new[] { "value4", "value5" })]
    public void DuplicatesBy_ReturnsExpected(IEnumerable<string> source, IEnumerable<string> values, IEnumerable<string> expectedKeys, IEnumerable<string> expectedValues)
    {
        // Arrange
        var sut = source.Zip(values).Select(x => (Key: x.First, Value: x.Second)).ToImmutableArray();

        // Act
        var result = sut.DuplicatesBy(x => x.Key);

        // Assert
        result.Should().ContainInOrder(expectedKeys.Zip(expectedValues).Select(x => (Key: x.First, Value: x.Second)));
    }

    #endregion
}
