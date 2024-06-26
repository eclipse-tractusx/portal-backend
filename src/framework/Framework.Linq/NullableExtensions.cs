/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;

public static class NullableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection) =>
        collection == null || !collection.Any();

    public static IEnumerable<KeyValuePair<TKey, TValue>> FilterNotNullValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue?>> source) =>
        source.Where(x => x.Value != null).Cast<KeyValuePair<TKey, TValue>>();

    public static IEnumerable<T> FilterNotNull<T>(this IEnumerable<T?> source) =>
        source.Where(x => x != null).Cast<T>();
}
