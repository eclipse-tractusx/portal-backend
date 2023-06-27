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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

public abstract class BaseOptionEnumerableValidation<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    protected ImmutableArray<Type> IgnoreTypes = new List<Type>
    {
        typeof(String),
        typeof(Boolean),
        typeof(Decimal),
    }.ToImmutableArray();

    protected readonly IConfiguration Section;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="section"></param>
    protected BaseOptionEnumerableValidation(string name, IConfiguration section)
    {
        Name = name;
        Section = section;
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

        var validationErrors = GetValidationErrors(options.GetType(), Section);

        return validationErrors.IfAny(
            errors => errors.Select(r => $"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'."),
            out var messages)
                ? ValidateOptionsResult.Fail(messages)
                : ValidateOptionsResult.Success;
    }

    private IEnumerable<ValidationResult> GetValidationErrors(Type type, IConfiguration configSection)
    {
        var validationResults = new List<ValidationResult>();
        foreach (var property in type
                     .GetProperties()
                     .Where(prop =>
                         !IgnoreTypes.Contains(prop.PropertyType) &&
                         !prop.PropertyType.IsEnum))
        {
            var propertyName = property.Name;
            validationResults.AddRange(ValidateAttribute(configSection, property, propertyName));
            validationResults.AddRange(CheckPropertiesOfProperty(configSection, property, propertyName));
        }

        foreach (var validationResult in validationResults)
        {
            yield return validationResult;
        }
    }

    protected abstract IEnumerable<ValidationResult> ValidateAttribute(IConfiguration config, PropertyInfo property, string propertyName);

    private IEnumerable<ValidationResult> CheckPropertiesOfProperty(IConfiguration configSection, PropertyInfo property, string propertyName)
    {
        var validationResults = new List<ValidationResult>();
        if (property.PropertyType.IsClass)
        {
            validationResults.AddRange(GetValidationErrors(property.PropertyType, configSection.GetSection(propertyName)));
        }
        else if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
        {
            var propertyType = property.PropertyType.GetGenericArguments().FirstOrDefault();
            if (propertyType == null || IgnoreTypes.Contains(propertyType) || propertyType.IsEnum)
                yield break;

            // Validate each item of the enumerable
            var listValues = (IEnumerable)configSection.GetSection(propertyName).Get(property.PropertyType);
            if (listValues != null)
            {
                var errors = Enumerable.Range(0, listValues.ToIEnumerable().Count())
                    .SelectMany(i => GetValidationErrors(propertyType, configSection.GetSection($"{propertyName}:{i}")));
                validationResults.AddRange(errors);
            }
        }

        foreach (var validationResult in validationResults)
        {
            yield return validationResult;
        }
    }
}
