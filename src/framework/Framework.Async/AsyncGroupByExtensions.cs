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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Async;

public static class AsyncGroupByExtensions
{
    public static async IAsyncEnumerable<IGrouping<TKey,TElement>> PreSortedGroupBy<T,TKey,TElement>(this IAsyncEnumerable<T> Data, Func<T,TKey> KeySelector, Func<T,TElement> ElementSelector) where T : notnull where TKey : notnull
    {
        var enumerator = Data.GetAsyncEnumerator();

        bool hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
        if (hasNext)
        {
            var key = KeySelector(enumerator.Current);
            TKey nextKey = default!;
            while(true)
            {
                var values = new LinkedList<TElement>();
                do
                {
                    values.AddLast(ElementSelector(enumerator.Current));
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                    if (hasNext)
                    {
                        nextKey = KeySelector(enumerator.Current);
                    }
                }
                while(hasNext && nextKey.Equals(key));
                yield return new Grouping<TKey,TElement>(key, values);
                if (!hasNext)
                {
                    yield break;
                }
                key = nextKey;
            }
        }
        yield break;
    }

    public static IAsyncEnumerable<IGrouping<TKey,T>> PreSortedGroupBy<T,TKey>(this IAsyncEnumerable<T> Data, Func<T,TKey> KeySelector) where T : notnull where TKey : notnull => Data.PreSortedGroupBy(KeySelector, x => x);

    sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public TKey Key { get; }

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            _elements = elements;
        }
        private readonly IEnumerable<TElement> _elements;

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => _elements.GetEnumerator();
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _elements.GetEnumerator();
    }    
}
