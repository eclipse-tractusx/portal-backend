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

using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq.Tests;

public class IfAnyTests
{
    private readonly IFixture _fixture;

    public IfAnyTests()
    {
        _fixture = new Fixture();
    }

    #region IfAny

    [Fact]
    public void IfAny_Empty_DoesNotIterateAndReturnsFalse()
    {
        // Arrange
        var elements = new List<string>();
        var sut = Enumerable.Empty<string>().AsFakeIEnumerable(out var enumerator);

        // Act
        var result = sut.IfAny(
            data =>
            {
                foreach (var x in data)
                {
                    elements.Add(x);
                }
            });

        // Assert
        result.Should().BeFalse();
        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappenedOnceExactly();
        elements.Should().BeEmpty();
    }

    [Fact]
    public void IfAny_NonEmpty_IteratesAndReturnsTrue()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(5).ToImmutableArray();

        var elements = new List<string>();
        var sut = data.AsFakeIEnumerable(out var enumerator);

        // Act
        var result = sut.IfAny(
            data =>
            {
                foreach (var x in data)
                {
                    elements.Add(x);
                }
            });

        // Assert
        result.Should().BeTrue();
        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(6, Times.Exactly);
        elements.Should().HaveCount(5).And.ContainInOrder(data);
    }

    [Fact]
    public void IfAny_PredicateNotFullfilled_IteratesAndReturnsFalse()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(5).ToImmutableArray();

        var elements = new List<string>();
        var sut = data.AsFakeIEnumerable(out var enumerator);

        // Act
        var result = sut.Where(x => false).IfAny(
            data =>
            {
                foreach (var x in data)
                {
                    elements.Add(x);
                }
            });

        // Assert
        result.Should().BeFalse();
        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(6, Times.Exactly);
        elements.Should().BeEmpty();
    }

    [Fact]
    public void IfAny_PredicateFullfilled_IteratesFilteredAndReturnsTrue()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(5).ToImmutableArray();

        var elements = new List<string>();
        var sut = data.AsFakeIEnumerable(out var enumerator);

        // Act
        var result = sut.Where(x => x == data[1] || x == data[3]).IfAny(
            data =>
            {
                foreach (var x in data)
                {
                    elements.Add(x);
                }
            });

        // Assert
        result.Should().BeTrue();
        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(6, Times.Exactly);
        elements.Should().HaveCount(2).And.ContainInOrder(new[] { data[1], data[3] });
    }

    #endregion
}
