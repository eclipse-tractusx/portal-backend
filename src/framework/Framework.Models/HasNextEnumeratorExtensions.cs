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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public static class HasNextEnumeratorExtensions
{
    private sealed class HasNextEnumeratorWrapper<T> : IHasNextEnumerator<T>, IDisposable
    {
        private readonly IEnumerator<T> _enumerator;
        private bool _hasNext;

        public HasNextEnumeratorWrapper(IEnumerable<T> enumerable)
        {
            _enumerator = enumerable.GetEnumerator();
            _hasNext = _enumerator.MoveNext();
        }

        public void Advance()
        {
            _hasNext = _enumerator.MoveNext();
        }

        public bool HasNext
        {
            get => _hasNext;
        }

        public T Current
        {
            get => _enumerator.Current;
        }

        public void Dispose() => _enumerator.Dispose();
    }

    public static IHasNextEnumerator<T> GetHasNextEnumerator<T>(this IEnumerable<T> source) => new HasNextEnumeratorWrapper<T>(source);
}

public interface IHasNextEnumerator<out T>
{
    void Advance();

    bool HasNext { get; }

    T Current { get; }
}
