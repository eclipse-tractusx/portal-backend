/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Keycloak.Net.Models.Users
{
	public class Credentials
	{
		[JsonProperty("algorithm")]
		public string Algorithm { get; set; }
		[JsonProperty("config")]
		public IDictionary<string, string> Config { get; set; }
		[JsonProperty("counter")]
		public int? Counter { get; set; }
		[JsonProperty("createdDate")]
		public long? CreatedDate { get; set; }
		[JsonProperty("device")]
		public string Device { get; set; }
		[JsonProperty("digits")]
		public int? Digits { get; set; }
		[JsonProperty("hashIterations")]
		public int? HashIterations { get; set; }
		[JsonProperty("hashSaltedValue")]
		public string HashSaltedValue { get; set; }
		[JsonProperty("period")]
		public int? Period { get; set; }
		[JsonProperty("salt")]
		public string Salt { get; set; }
		[JsonProperty("temporary")]
		public bool? Temporary { get; set; }
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
