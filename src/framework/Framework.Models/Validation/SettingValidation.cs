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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

/// <summary>
/// Extension methods for adding options related validation
/// </summary>
public static class SettingValidation
{
    /// <summary>
    /// Register this options instance for validation of the custom <see cref="EnumEnumerationAttribute"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    /// <param name="section">The current configuration section</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
    public static OptionsBuilder<TOptions> ValidateEnumEnumeration<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, IConfigurationSection section) where TOptions : class
    {
        optionsBuilder.Services.AddTransient<IValidateOptions<TOptions>>(_ => new EnumEnumerableValidation<TOptions>(optionsBuilder.Name, section));
        return optionsBuilder;
    }

    /// <summary>
    /// Register this options instance for validation of the custom <see cref="DistinctValuesAttribute"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    /// <param name="section">The current configuration section</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
    public static OptionsBuilder<TOptions> ValidateDistinctValues<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, IConfigurationSection section) where TOptions : class
    {
        optionsBuilder.Services.AddTransient<IValidateOptions<TOptions>>(_ => new DistinctValuesValidation<TOptions>(optionsBuilder.Name, section));
        return optionsBuilder;
    }
}
