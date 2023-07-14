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

    [Fact]
    public void IfAny_ReturnsSelectedBeingIteratedTwice_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<(string First, string Second)>(5).ToImmutableArray();

        // Act
        var result = data.IfAny(data => data.Skip(1).Take(3).Select(x => x.First), out var selected);

        // Assert
        result.Should().BeTrue();
        selected.Should().NotBeNull();

        // Act
        var firstRun = selected!.ToImmutableArray();

        // Assert
        firstRun.Should().HaveCount(3).And.ContainInOrder(data[1].First, data[2].First, data[3].First);

        // Act
        var secondRun = selected!.ToImmutableArray();

        // Assert
        secondRun.Should().HaveCount(3).And.ContainInOrder(data[1].First, data[2].First, data[3].First);
    }

    public class ResetableEnumerable<T> : IEnumerable<T>
    {
        private readonly T[] _items;

        public ResetableEnumerable(IEnumerable<T> items)
        {
            _items = items.ToArray();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => new ResetableEnumerator<T>(_items);
    }

    public class ResetableEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _items;

        public ResetableEnumerator(T[] items)
        {
            _items = items;
        }

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        private int position = -1;

        public bool MoveNext()
        {
            position++;
            return (position < _items.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current!;
            }
        }

        public T Current
        {
            get
            {
                try
                {
                    return _items[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {
        }
    }

    [Fact]
    public void IfAny_ReturnsSelectedBeingReset_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<string>(3).ToImmutableArray();
        var resetable = new ResetableEnumerable<string>(data);
        var sut = resetable.AsFakeIEnumerable(out var enumerator);

        // Act
        var result = sut.IfAny(data => data, out var selected);

        // Assert
        result.Should().BeTrue();
        selected.Should().NotBeNull();

        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappenedOnceExactly();

        // Act
        var result_enumerator = selected!.GetEnumerator();

        // Assert
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[0]);
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[1]);
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[2]);
        result_enumerator.MoveNext().Should().BeFalse();

        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(4, Times.Exactly);
        A.CallTo(() => enumerator.Reset())
            .MustNotHaveHappened();

        // Act
        result_enumerator.Reset();

        // Assert
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[0]);
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[1]);
        result_enumerator.MoveNext().Should().BeTrue();
        result_enumerator.Current.Should().Be(data[2]);
        result_enumerator.MoveNext().Should().BeFalse();

        A.CallTo(() => sut.GetEnumerator())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(8, Times.Exactly);
        A.CallTo(() => enumerator.Reset())
            .MustHaveHappenedOnceExactly();
    }

    #endregion
}
