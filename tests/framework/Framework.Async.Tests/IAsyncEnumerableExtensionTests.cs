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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Async.Tests;

public class AwaitAllIAsyncEnumerableExtensionTests
{
    private readonly IFixture _fixture = new Fixture();

    #region AwaitAll

    [Fact]
    public async Task TestAwaitAll_CallsAsyncEnumeratorExpectedNumberOfTimes()
    {
        var sut = _fixture.CreateMany<Guid>(10).AsFakeIAsyncEnumerable(out var asyncEnumerator);

        await sut.AwaitAll();

        A.CallTo(() => asyncEnumerator.MoveNextAsync()).MustHaveHappened(11, Times.Exactly);
        A.CallTo(() => asyncEnumerator.Current).MustNotHaveHappened();
    }

    #endregion

    #region CatchingAsync

    [Fact]
    public async Task TestCatchingAsyncClassType()
    {
        var message = _fixture.Create<string>();

        var data = _fixture.CreateMany<string>(5).ToImmutableArray();

        async IAsyncEnumerable<string> CreateSut()
        {
            var n = 0;
            foreach (var x in data)
            {
                if (n == 3)
                    throw new Exception(message);
                yield return x;
                n++;
            }
            await Task.CompletedTask;
        }

        var items = new List<string>();
        Exception? error = null;

        var sut = CreateSut();

        await foreach (var item in sut.CatchingAsync(exception =>
        {
            error = exception;
            return Task.CompletedTask;
        }))
        {
            items.Add(item);
        }

        error.Should().NotBeNull();
        error!.Message.Should().Be(message);

        items.Should().HaveCount(3)
            .And.ContainInOrder(data.Take(3));
    }

    [Fact]
    public async Task TestCatchingAsyncValueType()
    {
        var message = _fixture.Create<string>();

        var data = _fixture.CreateMany<Guid>(5).ToImmutableArray();

        async IAsyncEnumerable<Guid> CreateSut()
        {
            var n = 0;
            foreach (var x in data)
            {
                if (n == 3)
                    throw new Exception(message);
                yield return x;
                n++;
            }
            await Task.CompletedTask;
        }

        var items = new List<Guid>();
        Exception? error = null;

        var sut = CreateSut();

        await foreach (var item in sut.CatchingAsync(exception =>
        {
            error = exception;
            return Task.CompletedTask;
        }))
        {
            items.Add(item);
        }

        error.Should().NotBeNull();
        error!.Message.Should().Be(message);

        items.Should().HaveCount(3)
            .And.ContainInOrder(data.Take(3));
    }

    #endregion
}
