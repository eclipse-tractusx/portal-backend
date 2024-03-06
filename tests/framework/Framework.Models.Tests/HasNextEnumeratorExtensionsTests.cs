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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Tests;

public class HasNextEnumeratorExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void HasNextEnumerator_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(5).ToImmutableArray();
        var extra1 = _fixture.Create<string>();
        var extra2 = _fixture.Create<string>();
        var sut = data.AsFakeIEnumerable(out var enumerator).GetHasNextEnumerator();

        IEnumerable<(string, bool)> Act(IHasNextEnumerator<string> hasNextEnumerator)
        {
            while (hasNextEnumerator.HasNext)
            {
                yield return (hasNextEnumerator.Current, hasNextEnumerator.HasNext);
                hasNextEnumerator.Advance();
            }
            yield return (extra1, hasNextEnumerator.HasNext);
            hasNextEnumerator.Advance();
            yield return (extra2, hasNextEnumerator.HasNext);
            hasNextEnumerator.Dispose();
        }

        // Act
        var result = Act(sut).ToList();

        // Assert
        var expected = data.Select(x => (x, true)).Append((extra1, false)).Append((extra2, false));
        result.Should().HaveSameCount(expected).And.ContainInOrder(expected);
        A.CallTo(() => enumerator.MoveNext()).MustHaveHappened(7, Times.Exactly);
        A.CallTo(() => enumerator.Current).MustHaveHappened(5, Times.Exactly);
        A.CallTo(() => enumerator.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void HasNextEnumerator_ThrowsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(5).ToImmutableArray();
        var sut = data.AsFakeIEnumerable(out var enumerator).GetHasNextEnumerator();

        static IEnumerable<string> Act(IHasNextEnumerator<string> hasNextEnumerator)
        {
            while (hasNextEnumerator.HasNext)
            {
                yield return hasNextEnumerator.Current;
                hasNextEnumerator.Advance();
            }
            yield return hasNextEnumerator.Current;
        }

        // Act
        Assert.Throws<InvalidOperationException>(() => Act(sut).ToList());

        // Assert
        A.CallTo(() => enumerator.MoveNext()).MustHaveHappened(6, Times.Exactly);
        A.CallTo(() => enumerator.Current).MustHaveHappened(6, Times.Exactly);
    }
}
