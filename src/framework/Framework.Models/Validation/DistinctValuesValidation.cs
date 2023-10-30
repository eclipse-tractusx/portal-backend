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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

/// <summary>
/// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
/// </summary>
/// <typeparam name="TOptions">The instance being validated.</typeparam>
public class DistinctValuesValidation<TOptions> : BaseOptionEnumerableValidation<TOptions> where TOptions : class
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="config">The configuration</param>
    public DistinctValuesValidation(string name, IConfiguration config)
        : base(name, config)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ValidationResult> ValidateAttribute(IConfiguration config, PropertyInfo property, string propertyName)
    {
        if (!Attribute.IsDefined(property, typeof(DistinctValuesAttribute)))
            yield break;

        var attribute = (DistinctValuesAttribute)property.GetCustomAttributes(typeof(DistinctValuesAttribute), false).Single();

        if (!property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
        {
            throw new UnexpectedConditionException(
                $"Attribute DistinctValues is applied to property {property.Name} which is not an IEnumerable type ({property.PropertyType})");
        }
        var listValues = config.GetSection(propertyName).Get(property.PropertyType) as IEnumerable;
        var items = listValues?.ToIEnumerable();

        IEnumerable<object> duplicates;

        switch (attribute.Selector, items)
        {
            case (null, null):
                yield break;
            case (null, _):
                duplicates = items.Duplicates();
                break;
            case (_, null):
                GetDelegate(attribute.Selector, property);
                yield break;
            default:
                var selector = GetDelegate(attribute.Selector, property);
                duplicates = items.DuplicatesBy(x => selector.DynamicInvoke(x));
                break;
        }

        if (duplicates.IfAny(dup => $"{string.Join(",", dup)} are duplicate values for {property.Name}.",
                out var message))
        {
            yield return new ValidationResult(message, new[] { property.Name });
        }
    }

    private static Delegate GetDelegate(string selector, PropertyInfo propertyInfo) =>
        selector.ToSelectorDelegate(propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault() ?? throw new UnexpectedConditionException($"cannot get generic arguments of {propertyInfo.PropertyType}"));
}
