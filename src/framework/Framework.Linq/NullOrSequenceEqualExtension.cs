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

using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;

public static class NullOrSequenceEqualExtensions
{
    public static bool NullOrContentEqual<T>(this IEnumerable<T>? items, IEnumerable<T>? others, IEqualityComparer<T>? comparer = null) where T : IComparable =>
        items == null && others == null ||
        items != null && others != null &&
        items.OrderBy(x => x).SequenceEqual(others.OrderBy(x => x), comparer);

    public static bool NullOrContentEqual<K, V>(this IEnumerable<KeyValuePair<K, V>>? items, IEnumerable<KeyValuePair<K, V>>? others, IEqualityComparer<KeyValuePair<K, V>>? comparer = null) where K : IComparable where V : IComparable =>
        items == null && others == null ||
        items != null && others != null &&
        items.OrderBy(x => x.Key).SequenceEqual(others.OrderBy(x => x.Key), comparer ?? new KeyValuePairEqualityComparer<K, V>());

    public static bool NullOrContentEqual<K, V>(this IEnumerable<KeyValuePair<K, IEnumerable<V>>>? items, IEnumerable<KeyValuePair<K, IEnumerable<V>>>? others, IEqualityComparer<KeyValuePair<K, IEnumerable<V>>>? comparer = null) where V : IComparable =>
        items == null && others == null ||
        items != null && others != null &&
        items.OrderBy(x => x.Key).SequenceEqual(others.OrderBy(x => x.Key), comparer ?? new EnumerableValueKeyValuePairEqualityComparer<K, V>());
}

public class KeyValuePairEqualityComparer<K, V> : IEqualityComparer<KeyValuePair<K, V>> where K : IComparable where V : IComparable
{
    public bool Equals(KeyValuePair<K, V> x, KeyValuePair<K, V> y) =>
        x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
    public int GetHashCode([DisallowNull] KeyValuePair<K, V> obj) => throw new NotImplementedException();
}

public class EnumerableValueKeyValuePairEqualityComparer<K, V> : IEqualityComparer<KeyValuePair<K, IEnumerable<V>>> where V : IComparable
{
    public bool Equals(KeyValuePair<K, IEnumerable<V>> source, KeyValuePair<K, IEnumerable<V>> other) =>
        (source.Key == null && other.Key == null ||
        source.Key != null && source.Key.Equals(other.Key)) &&
        source.Value.NullOrContentEqual(other.Value);

    public int GetHashCode([DisallowNull] KeyValuePair<K, IEnumerable<V>> obj) => throw new NotImplementedException();
}
