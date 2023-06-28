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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

/// <summary>
/// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
/// </summary>
/// <typeparam name="TOptions">The instance being validated.</typeparam>
public class EnumEnumerableValidation<TOptions> : BaseOptionEnumerableValidation<TOptions> where TOptions : class
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="section"></param>
    public EnumEnumerableValidation(string name, IConfiguration section)
        : base(name, section)
    {
    }

    protected override IEnumerable<ValidationResult> ValidateAttribute(IConfiguration config, PropertyInfo property, string propertyName)
    {
        if (!Attribute.IsDefined(property, typeof(EnumEnumerationAttribute)))
            yield break;

        var section = config.GetSection(propertyName);
        var configuredValues = section.Get<IEnumerable<string>>();
        if (!property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
        {
            throw new UnexpectedConditionException(
                $"Attribute EnumEnumeration is applied to property {propertyName} which is not an IEnumerable type ({property.PropertyType})");
        }

        var propertyType = property.PropertyType.GetGenericArguments().FirstOrDefault();
        if (propertyType is not { IsEnum: true })
        {
            throw new UnexpectedConditionException(
                $"{propertyName} must be of type IEnumerable<Enum> but is IEnumerable<{propertyType}>");
        }

        var notMatchingValues = configuredValues.Except(propertyType.GetEnumNames());
        if (notMatchingValues.IfAny(
                values =>
                    $"{string.Join(",", values)} is not a valid value for {propertyType} in section {section.Path}. Valid values are: {string.Join(", ", propertyType.GetEnumNames())}",
                out var message))
        {
            yield return new ValidationResult(message, new[] { propertyName });
        }
    }
}
