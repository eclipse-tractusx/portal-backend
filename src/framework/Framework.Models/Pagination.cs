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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public class Pagination
{
    public class Response<T>
    {
        public Response(Metadata meta, IEnumerable<T> content)
        {
            Meta = meta;
            Content = content;
        }

        [JsonPropertyName("meta")]
        public Metadata Meta { get; set; }

        [JsonPropertyName("content")]
        public IEnumerable<T> Content { get; set; }
    }

    public class Metadata
    {
        public Metadata(int numberOfElements, int numberOfPages, int page, int pageSize)
        {
            NumberOfElements = numberOfElements;
            NumberOfPages = numberOfPages;
            Page = page;
            PageSize = pageSize;
        }

        [JsonPropertyName("totalElements")]
        public int NumberOfElements { get; }

        [JsonPropertyName("totalPages")]
        public int NumberOfPages { get; }

        [JsonPropertyName("page")]
        public int Page { get; }

        [JsonPropertyName("contentSize")]
        public int PageSize { get; }
    }

    public class AsyncSource<T>
    {
        public AsyncSource(Task<int> count, IAsyncEnumerable<T> data)
        {
            Count = count;
            Data = data;
        }

        public Task<int> Count { get; }
        public IAsyncEnumerable<T> Data { get; }
    }

    public class Source<T>
    {
        public Source(int count, IEnumerable<T> data)
        {
            Count = count;
            Data = data;
        }

        public int Count { get; }
        public IEnumerable<T> Data { get; }
    }

    public static async Task<Response<T>> CreateResponseAsync<T>(int page, int size, int maxSize, Func<int, int, AsyncSource<T>> getSource)
    {
        ValidatePaginationParameters(page, size, maxSize);

        var source = getSource(size * page, size);
        var count = await source.Count.ConfigureAwait(false);
        var data = await source.Data.ToListAsync().ConfigureAwait(false);

        return new Response<T>(
            new Metadata(
                count,
                count / size + Math.Clamp(count % size, 0, 1),
                page,
                data.Count()),
            data);
    }

    public static async Task<Response<T>> CreateResponseAsync<T>(int page, int size, int maxSize, Func<int, int, Task<Source<T>?>> getSource)
    {
        ValidatePaginationParameters(page, size, maxSize);

        var source = await getSource(size * page, size).ConfigureAwait(false);
        return source == null
            ? new Response<T>(new Metadata(0, 0, 0, 0), Enumerable.Empty<T>())
            : new Response<T>(
                new Metadata(
                    source.Count,
                    source.Count / size + Math.Clamp(source.Count % size, 0, 1),
                    page,
                    source.Data.Count()),
                source.Data);
    }

    public static IQueryable<Pagination.Source<T>?> CreateSourceQueryAsync<TEntity, TKey, T>(int skip, int take, IQueryable<IGrouping<TKey, TEntity>> query, Expression<Func<IEnumerable<TEntity>, IOrderedEnumerable<TEntity>>>? orderBy, Expression<Func<TEntity, T>> select) where TEntity : class
    {
        var paramGroup = Expression.Parameter(typeof(IGrouping<TKey, TEntity>), "group");

        var selector = Expression.Lambda<Func<IGrouping<TKey, TEntity>, Pagination.Source<T>>>(
            Expression.New(typeof(Pagination.Source<T>).GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, new[] { typeof(int), typeof(IEnumerable<T>) })!,
                new Expression[] {
                    Expression.Call(typeof(Enumerable), "Count", new Type[] { typeof(TEntity) }, paramGroup),
                    Expression.Call(typeof(Enumerable), "Select", new Type[] { typeof(TEntity), typeof(T) }, new Expression[] {
                        Expression.Call(typeof(Enumerable), "Take", new Type[] { typeof(TEntity) }, new Expression [] {
                            Expression.Call(typeof(Enumerable), "Skip", new Type[] { typeof(TEntity) }, new Expression[] {
                                orderBy == null ? paramGroup : Expression.Invoke(orderBy, paramGroup),
                                Expression.Constant(skip) }),
                            Expression.Constant(take) }),
                        select })
                }
            ),
            paramGroup);

        return query.Select(selector);
    }

    private static void ValidatePaginationParameters(int page, int size, int maxSize)
    {
        if (page < 0)
        {
            throw new ControllerArgumentException("Parameter page must be >= 0", nameof(page));
        }
        if (size <= 0 || size > maxSize)
        {
            throw new ControllerArgumentException($"Parameter size must be between 1 and {maxSize}", nameof(size));
        }
    }
}
