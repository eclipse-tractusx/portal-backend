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

using FakeItEasy;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

public static class FakeIAsyncEnumerableExtensions
{
    public static IAsyncEnumerable<T> AsFakeIAsyncEnumerable<T>(this IEnumerable<T> enumerable, out IAsyncEnumerator<T> outAsyncEnumerator)
    {
        IEnumerator<T>? enumerator = null;
        var fakeEnumerable = A.Fake<IAsyncEnumerable<T>>();
        var fakeEnumerator = A.Fake<IAsyncEnumerator<T>>();

        A.CallTo(() => fakeEnumerable.GetAsyncEnumerator(A<CancellationToken>._))
            .ReturnsLazily((CancellationToken _) =>
            {
                if (enumerator != null)
                    throw new InvalidOperationException();
                enumerator = enumerable.GetEnumerator();
                return fakeEnumerator;
            });

        A.CallTo(() => fakeEnumerator.MoveNextAsync())
            .ReturnsLazily(() => (enumerator ?? throw new InvalidOperationException()).MoveNext());

        A.CallTo(() => fakeEnumerator.Current)
            .ReturnsLazily(() => (enumerator ?? throw new InvalidOperationException()).Current);

        outAsyncEnumerator = fakeEnumerator;
        return fakeEnumerable;
    }

    public static IEnumerable<T> AsFakeIEnumerable<T>(this IEnumerable<T> enumerable, out IEnumerator<T> outEnumerator)
    {
        IEnumerator<T>? enumerator = null;
        var fakeEnumerable = A.Fake<IEnumerable<T>>();
        var fakeEnumerator = A.Fake<IEnumerator<T>>();

        A.CallTo(() => fakeEnumerable.GetEnumerator())
            .ReturnsLazily(() =>
            {
                if (enumerator != null)
                    throw new InvalidOperationException();
                enumerator = enumerable.GetEnumerator();
                return fakeEnumerator;
            });

        A.CallTo(() => fakeEnumerator.MoveNext())
            .ReturnsLazily(() => (enumerator ?? throw new InvalidOperationException()).MoveNext());

        A.CallTo(() => fakeEnumerator.Current)
            .ReturnsLazily(() => (enumerator ?? throw new InvalidOperationException()).Current);

        outEnumerator = fakeEnumerator;
        return fakeEnumerable;
    }
}
