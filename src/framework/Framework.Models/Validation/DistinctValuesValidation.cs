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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

/// <summary>
/// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
/// </summary>
/// <typeparam name="TOptions">The instance being validated.</typeparam>
public class DistinctValuesValidation<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    public DistinctValuesValidation(string name)
    {
        Name = name;
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

        IEnumerable<ValidationResult> GetValidationResults()
        {
            foreach (var propertyInfo in options.GetType()
                            .GetProperties()
                            .Where(prop => Attribute.IsDefined(prop, typeof(DistinctValuesAttribute))))
            {
                var attribute = (DistinctValuesAttribute)propertyInfo.GetCustomAttributes(typeof(DistinctValuesAttribute), false).Single();

                if (!propertyInfo.PropertyType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable))) throw new UnexpectedConditionException($"Attribute DistinceValues is applied to property {propertyInfo.Name} which is not an IEnumerable type ({propertyInfo.PropertyType})");

                var items = propertyInfo.GetGetMethod()?.Invoke(options, new object[] { })?.ToIEnumerable() ?? throw new UnexpectedConditionException($"cannot access property getter of {propertyInfo.Name}");

                var duplicates = attribute.Selector == null
                    ? items.Duplicates()
                    : GetGenericDuplicates(propertyInfo, attribute.Selector, items);

                if (duplicates.Any())
                {
                    yield return new($"{string.Join(",", duplicates)} are duplicate values for {propertyInfo.Name}.", new[] { propertyInfo.Name });
                }
            }
        }

        var validationResults = GetValidationResults();

        return validationResults.Any() ?
            ValidateOptionsResult.Fail(validationResults.Select(r => $"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'.")) :
            ValidateOptionsResult.Success;
    }
    private static IEnumerable<object> GetGenericDuplicates(PropertyInfo info, string selector, IEnumerable<object> items)
    {
        var genericType = info.PropertyType.GetGenericArguments().FirstOrDefault() ?? throw new UnexpectedConditionException($"cannot get generic arguments of {info.Name}");
    
        var genericMethod = typeof(CSharpScript).GetMethods().Where(method => method.Name == "EvaluateAsync" && method.IsGenericMethod).SingleOrDefault()?
            .MakeGenericMethod(new [] { typeof(Func<,>).MakeGenericType(genericType, typeof(object)) }) ?? throw new UnexpectedConditionException($"unable to access method CSharpScript.EvaluateAsync for {genericType}");

        var task = genericMethod.Invoke(null, new object?[] { selector, null, null, null, CancellationToken.None });
        var selectorDelegate = task?.GetType().GetProperty("Result")?.GetGetMethod()?.Invoke(task, Array.Empty<object?>()) ?? throw new UnexpectedConditionException($"failed to evaluate selector {selector} of {info.Name}");
        return items.DuplicatesBy(x => (selectorDelegate as Delegate)?.DynamicInvoke(x)) ?? throw new UnexpectedConditionException($"failed to execute DuplicatesBy on {items} using selector {selectorDelegate}");
    }
}
