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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Async;

public static class AsyncIfAnyExtension
{
    private sealed class AsyncIfAnyEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _source;
        private readonly IAsyncEnumerator<T> _enumerator;
        private bool _isFirst;

        public AsyncIfAnyEnumerable(IAsyncEnumerable<T> source, IAsyncEnumerator<T> enumerator)
        {
            _source = source;
            _enumerator = enumerator;
            _isFirst = true;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (_isFirst)
            {
                _isFirst = false;
                return new AsyncIfAnyEnumerator<T>(_enumerator);
            }
            return _source.GetAsyncEnumerator(cancellationToken);
        }
    }

    private sealed class AsyncIfAnyEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _enumerator;
        private bool _isFirst;

        public AsyncIfAnyEnumerator(IAsyncEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
            _isFirst = true;
        }

        public T Current => _enumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return ValueTask.FromResult(true);
            }
            return _enumerator.MoveNextAsync();
        }

        public ValueTask DisposeAsync() => _enumerator.DisposeAsync();
    }

    public static async ValueTask<bool> IfAny<T>(this IAsyncEnumerable<T> source, Action<IAsyncEnumerable<T>> action, CancellationToken cancellationToken = default)
    {
        var enumerator = source.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            action(new AsyncIfAnyEnumerable<T>(source, enumerator));
            return true;
        }

        return false;
    }
}
