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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;

public static class IfAnyExtension
{
    private sealed class IfAnyEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly IEnumerator<T> _enumerator;
        private bool isFirst;

        public IfAnyEnumerable(IEnumerable<T> source, IEnumerator<T> enumerator)
        {
            _source = source;
            _enumerator = enumerator;
            isFirst = true;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            if (isFirst)
            {
                isFirst = false;
                return new IfAnyEnumerator<T>(_enumerator);
            }
            return _source.GetEnumerator();
        }
    }

    private sealed class IfAnyEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private bool isFirst;

        public IfAnyEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
            isFirst = true;
        }

        public T Current => _enumerator.Current;

        object System.Collections.IEnumerator.Current => (_enumerator as System.Collections.IEnumerator).Current;

        public bool MoveNext()
        {
            if (isFirst)
            {
                isFirst = false;
                return true;
            }
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            isFirst = false;
            _enumerator.Reset();
        }

        public void Dispose() => _enumerator.Dispose();
    }

    public static bool IfAny<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action)
    {
        var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext())
        {
            action(new IfAnyEnumerable<T>(source, enumerator));
            return true;
        }

        return false;
    }

    public static bool IfAny<T, R>(this IEnumerable<T> source, Func<IEnumerable<T>, R> process, out R? returnValue) where R : class?
    {
        var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext())
        {
            returnValue = process(new IfAnyEnumerable<T>(source, enumerator));
            return true;
        }

        returnValue = null;
        return false;
    }
}
