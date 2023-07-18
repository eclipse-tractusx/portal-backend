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

public static class AsyncAnyExtensions
{
    public static async Task<bool> AnyAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        foreach (var item in source)
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                return true;
            }
        }
        return false;
    }

    public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                return true;
            }
        }
        return false;
    }

    public static async Task<bool> AnyAsync(this IAsyncEnumerable<bool> source)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            if (item)
            {
                return true;
            }
        }
        return false;
    }

    public static async Task<bool> AllAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        foreach (var item in source)
        {
            if (!(await predicate(item).ConfigureAwait(false)))
            {
                return false;
            }
        }
        return true;
    }

    public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            if (!(await predicate(item).ConfigureAwait(false)))
            {
                return false;
            }
        }
        return true;
    }
}
