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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

/// <summary>
/// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
/// </summary>
/// <typeparam name="TOptions">The instance being validated.</typeparam>
public class EnumEnumerableValidation<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IConfiguration _section;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="section"></param>
    public EnumEnumerableValidation(string name, IConfiguration section)
    {
        Name = name;
        _section = section;
    }

    /// <summary>
    /// The options name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Validates a specific named options instance (or all when <paramref name="name"/> is null).
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The <see cref="ValidateOptionsResult"/> result.</returns>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Null name is used to configure all named options.
        if (name == null || name != Name)
        {
            return ValidateOptionsResult.Skip;
        }

        var validationErrors = GetValidationErrors(options);

        return validationErrors.IfAny(
            errors => errors.Select(r => $"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'."),
            out var messages)
                ? ValidateOptionsResult.Fail(messages)
                : ValidateOptionsResult.Success;
    }

    private IEnumerable<ValidationResult> GetValidationErrors(TOptions options)
    {
        foreach (var propertyInfo in options.GetType()
                        .GetProperties()
                        .Where(prop => Attribute.IsDefined(prop, typeof(EnumEnumerationAttribute))))
        {
            var configuredValues = new List<string>();
            _section.GetSection(propertyInfo.Name).Bind(configuredValues);

            if (!propertyInfo.PropertyType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)))
            {
                throw new UnexpectedConditionException($"Attribute EnumEnumeration is applied to property {propertyInfo.Name} which is not an IEnumerable type ({propertyInfo.PropertyType})");
            }

            var propertyType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
            if (propertyType is not { IsEnum: true })
            {
                throw new UnexpectedConditionException($"{propertyInfo.Name} must be of type IEnumerable<Enum> but is IEnumerable<{propertyType}>");
            }

            var notMatchingValues = configuredValues.Except(propertyType.GetEnumNames());
            if (notMatchingValues.IfAny(
                values => $"{string.Join(",", values)} is not a valid value for {propertyType}. Valid values are: {string.Join(", ", propertyType.GetEnumNames())}",
                out var message))
            {
                yield return new(message, new[] { propertyInfo.Name });
            }
        }
    }
}
