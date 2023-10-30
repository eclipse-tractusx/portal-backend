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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;

public static class ObjectToEnumerableExtension
{
    public static IEnumerable<object> ToIEnumerable(this object value)
    {
        var enumerator = value?.GetType().GetMethod("GetEnumerator")?.Invoke(value, null) ?? throw new ArgumentException($"object instance does not implement IEnumerable ({value?.GetType()})");
        var moveNext = enumerator?.GetType().GetMethod("MoveNext") ?? throw new UnexpectedConditionException("method 'moveNext' should never be null here");
        var current = enumerator?.GetType().GetProperty("Current")?.GetMethod ?? throw new UnexpectedConditionException("property 'Current' should never be null here");
        while (true)
        {
            var hasNext = moveNext.Invoke(enumerator, null) ?? throw new UnexpectedConditionException($"failed to enumerate object {value}: moveNext should never return null here");
            if (hasNext is not true)
            {
                yield break;
            }
            yield return current.Invoke(enumerator, null) ?? throw new UnexpectedConditionException($"failed to enumerate object {value}: Current should never return null here");
        }
    }
}
