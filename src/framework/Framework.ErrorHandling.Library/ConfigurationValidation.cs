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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

public class ConfigurationValidation<TSettings>
{
	public ConfigurationValidation<TSettings> NotNull(object? item, Func<string> getItemName)
	{
		if (item == null)
		{
			throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
		}
		return this;
	}

	public ConfigurationValidation<TSettings> NotDefault(object item, Func<string> getItemName)
	{
		if (item == default)
		{
			throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
		}
		return this;
	}

	public ConfigurationValidation<TSettings> NotNullOrEmpty(string? item, Func<string> getItemName)
	{
		if (string.IsNullOrEmpty(item))
		{
			throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or empty");
		}
		return this;
	}

	public ConfigurationValidation<TSettings> NotNullOrWhiteSpace(string? item, Func<string> getItemName)
	{
		if (string.IsNullOrWhiteSpace(item))
		{
			throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or whitespace");
		}
		return this;
	}
}
