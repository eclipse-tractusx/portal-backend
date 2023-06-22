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
    public static bool IfAny<T>(this IEnumerable<T> source, Action<IEnumerable<T>> action)
    {
        var enumerator = source.GetEnumerator();

        IEnumerable<T> Iterate()
        {
            yield return enumerator.Current;

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        if (enumerator.MoveNext())
        {
            action(Iterate());
            return true;
        }

        return false;
    }

    public static bool IfAny<T, R>(this IEnumerable<T> source, Func<IEnumerable<T>, R> process, out R? returnValue) where R : class?
    {
        var enumerator = source.GetEnumerator();

        IEnumerable<T> Iterate()
        {
            yield return enumerator.Current;

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        if (enumerator.MoveNext())
        {
            returnValue = process(Iterate());
            return true;
        }

        returnValue = null;
        return false;
    }
}
