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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public class ValidateEnumValuesAttribute : ValidationAttribute
{
    override protected ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var type = value?
            .GetType()
            .GetInterfaces()
            .Where(t => t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(t => t.GetGenericArguments()[0])
            .FirstOrDefault();
        if (type is null || !type.IsEnum)
        {
            throw new UnexpectedConditionException($"invalid use of attribute ValidateEnumValues: {validationContext.MemberName}, value {validationContext.ObjectInstance} is of type {validationContext.ObjectType} which is not an IEnumerable of Enum-type");
        }
        var values = type.GetEnumValues();
        var enumerator = value?.GetType().GetMethod("GetEnumerator")?.Invoke(value, null);
        var moveNext = enumerator?.GetType().GetMethod("MoveNext");
        var current = enumerator?.GetType().GetProperty("Current")?.GetMethod;
        if (enumerator is null || moveNext is null || current is null)
        {
            throw new UnexpectedConditionException($"attribute ValidateEnumValues failed to enumerate {validationContext.MemberName}: enumerator, moveNext or current should never be null here");
        }
        while (true)
        {
            var hasNext = moveNext.Invoke(enumerator,null);
            if (hasNext is null)
            {
                throw new UnexpectedConditionException($"attribute ValidateEnumValues failed to enumerate {validationContext.MemberName}: hasNext should never be null here");
            }
            if (hasNext is not true)
            {
                return null;
            }
            var item = current.Invoke(enumerator,null);
            if (item is null || Array.BinarySearch(values, item) < 0)
            {
                return new ValidationResult($"{item} is not a valid value for {type}. Valid values are: {string.Join(", ", type.GetEnumNames())}");
            }
        }
    }
}
