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
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq.Tests;

public class NullOrSequenceEqualExtensionsTests
{
    [Theory]
    [InlineData(new[] { "a", "b", "c" }, new[] { "c", "b", "a" }, true)]
    [InlineData(null, new[] { "c", "b", "a" }, false)]
    [InlineData(new[] { "a", "b", "c" }, null, false)]
    [InlineData(null, null, true)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b", "c", "x" }, false)]
    [InlineData(new[] { "a", "b", "c", "x" }, new[] { "a", "b", "c" }, false)]
    public void NullOrContentEqual_ReturnsExpected(IEnumerable<string>? first, IEnumerable<string>? second, bool expected)
    {
        // Act
        var result = first.NullOrContentEqual(second);

        // Assert
        result.Should().Be(expected);
    }

    private class TestComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => x == y;
        public int GetHashCode([DisallowNull] string obj) => throw new NotImplementedException();
    }

    [Theory]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b", "c" }, true)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "c", "b", "a" }, true)]
    [InlineData(null, new[] { "c", "b", "a" }, false)]
    [InlineData(new[] { "a", "b", "c" }, null, false)]
    [InlineData(null, null, true)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b", "c", "x" }, false)]
    [InlineData(new[] { "a", "b", "c", "x" }, new[] { "a", "b", "c" }, false)]
    public void NullOrContentEqual_WithComparer_ReturnsExpected(IEnumerable<string>? first, IEnumerable<string>? second, bool expected)
    {
        // Act
        var result = first.NullOrContentEqual(second, new TestComparer());

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, true)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, new[] { "c", "b", "a" }, new[] { "cv", "bv", "av" }, true)]
    [InlineData(null, null, new[] { "c", "b", "a" }, new[] { "cv", "bv", "av" }, false)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, null, null, false)]
    [InlineData(null, null, null, null, true)]
    [InlineData(new[] { "a", "b", "c", "x" }, new[] { "av", "bv", "cv", "xv" }, new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, false)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, new[] { "a", "b", "c", "x" }, new[] { "av", "bv", "cv", "xv" }, false)]
    [InlineData(new[] { "a", "b", "x" }, new[] { "av", "bv", "cv" }, new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, false)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "av", "bv", "xv" }, new[] { "a", "b", "c" }, new[] { "av", "bv", "cv" }, false)]
    public void NullOrContentEqual_WithKeyValuePairs_ReturnsExpected(IEnumerable<string>? first, IEnumerable<string>? firstValues, IEnumerable<string>? second, IEnumerable<string>? secondValues, bool expected)
    {
        // Arrange
        var firstItems = first?.Zip(firstValues ?? throw new UnexpectedConditionException("firstValues should never be null here"), (x, y) => new KeyValuePair<string, string>(x, y));
        var secondItems = second?.Zip(secondValues ?? throw new UnexpectedConditionException("secondValues should never be null here"), (x, y) => new KeyValuePair<string, string>(x, y));

        // Act
        var result = firstItems.NullOrContentEqual(secondItems);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, true)]
    [InlineData(new[] { "c", "b", "a" }, new[] { "c1", "c2", "c3" }, new[] { "b1", "b2", "b3" }, new[] { "a1", "a2", "a3" }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, true)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a3", "a2", "a1" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, true)]
    [InlineData(new[] { "a", "b", "x" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, false)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a1", "a2", "x3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, false)]
    [InlineData(null, new string[] { }, new string[] { }, new string[] { }, new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, false)]
    [InlineData(new[] { "a", "b", "c" }, new[] { "a1", "a2", "a3" }, new[] { "b1", "b2", "b3" }, new[] { "c1", "c2", "c3" }, null, new string[] { }, new string[] { }, new string[] { }, false)]
    [InlineData(null, new string[] { }, new string[] { }, new string[] { }, null, new string[] { }, new string[] { }, new string[] { }, true)]
    public void NullOrContentEqual_WithKeyValuePairEnumerables_ReturnsExpected(IEnumerable<string>? first, IEnumerable<string> firstFirstValues, IEnumerable<string> firstSecondValues, IEnumerable<string> firstThirdValues, IEnumerable<string>? second, IEnumerable<string> secondFirstValues, IEnumerable<string> secondSecondValues, IEnumerable<string> secondThirdValues, bool expected)
    {
        // Arrange
        var firstItems = first?.Zip(new[] { firstFirstValues, firstSecondValues, firstThirdValues }, (x, y) => new KeyValuePair<string, IEnumerable<string>>(x, y));
        var secondItems = second?.Zip(new[] { secondFirstValues, secondSecondValues, secondThirdValues }, (x, y) => new KeyValuePair<string, IEnumerable<string>>(x, y));

        // Act
        var result = firstItems.NullOrContentEqual(secondItems);

        // Assert
        result.Should().Be(expected);
    }
}
