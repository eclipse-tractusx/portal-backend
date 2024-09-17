/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Async;

public static class AsyncAggregateExtensions
{
    public static Task<TAccumulate> AggregateAwait<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulate, CancellationToken cancellationToken = default)
    {
        using var enumerator = source.GetEnumerator();
        return AggregateAwait(enumerator, seed, accumulate, cancellationToken);
    }

    public static Task<TAccumulate> AggregateAwait<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulate, CancellationToken cancellationToken = default)
    {
        using var enumerator = source.GetEnumerator();
        return AggregateAwait(enumerator, seed, accumulate, cancellationToken);
    }

    public static async Task<TResult> AggregateAwait<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulate, Func<TAccumulate, TResult> result, CancellationToken cancellationToken = default) =>
        result(await AggregateAwait(source, seed, accumulate, cancellationToken));

    public static async Task<TResult> AggregateAwait<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulate, Func<TAccumulate, TResult> result, CancellationToken cancellationToken = default) =>
        result(await AggregateAwait(source, seed, accumulate, cancellationToken));

    public static Task<TSource> AggregateAwait<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, Task<TSource>> accumulate, CancellationToken cancellationToken = default)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("source must not be empty");

        return AggregateAwait(enumerator, enumerator.Current, accumulate, cancellationToken);
    }

    public static Task<TSource> AggregateAwait<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, Task<TSource>> accumulate, CancellationToken cancellationToken = default)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("source must not be empty");

        return AggregateAwait(enumerator, enumerator.Current, accumulate, cancellationToken);
    }

    private static async Task<TAccumulate> AggregateAwait<TSource, TAccumulate>(IEnumerator<TSource> enumerator, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulate, CancellationToken cancellationToken)
    {
        var accumulator = seed;
        while (enumerator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            accumulator = await accumulate(accumulator, enumerator.Current, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        return accumulator;
    }

    private static async Task<TAccumulate> AggregateAwait<TSource, TAccumulate>(IEnumerator<TSource> enumerator, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulate, CancellationToken cancellationToken)
    {
        var accumulator = seed;
        while (enumerator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            accumulator = await accumulate(accumulator, enumerator.Current).ConfigureAwait(ConfigureAwaitOptions.None);
        }
        return accumulator;
    }
}
