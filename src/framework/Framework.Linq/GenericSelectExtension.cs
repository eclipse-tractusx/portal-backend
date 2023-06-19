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

using Microsoft.CodeAnalysis.CSharp.Scripting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;

public static class GenericSelectExtension
{
    public static Delegate ToSelectorDelegate(this string selector, Type type)
    {
        var genericMethod = typeof(CSharpScript).GetMethods().SingleOrDefault(method => method.Name == "EvaluateAsync" && method.IsGenericMethod)?
            .MakeGenericMethod(typeof(Func<,>).MakeGenericType(type, typeof(object))) ?? throw new UnexpectedConditionException($"unable to access method CSharpScript.EvaluateAsync for {type}");

        try
        {
            var task = genericMethod.Invoke(null, new object?[] { selector, null, null, null, CancellationToken.None });
            return task?.GetType().GetProperty("Result")?.GetGetMethod()?.Invoke(task, Array.Empty<object?>()) as Delegate ?? throw new UnexpectedConditionException($"failed to evaluate selector {selector} for {type}");
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException == null
                ? new UnexpectedConditionException($"invalid selector {selector} for type {type}")
                : new UnexpectedConditionException($"invalid selector {selector} for type {type}", e.InnerException);
        }
    }
}
