/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Model
{

	public class Company
	{
		public string bpn { get; set; }
		public string parent { get; set; }
		public string accountGroup { get; set; }
		public string name1 { get; set; }
		public string name2 { get; set; }
		public string name3 { get; set; }
		public string name4 { get; set; }
		public string addressVersion { get; set; }
		public string country { get; set; }
		public string city { get; set; }
		public int postalCode { get; set; }
		public string street1 { get; set; }
		public string street2 { get; set; }
		public string street3 { get; set; }
		public int houseNumber { get; set; }
		public string taxNumber1 { get; set; }
		public string taxNumber1Type { get; set; }
		public string taxNumber2 { get; set; }
		public string taxNumber2Type { get; set; }
		public string taxNumber3 { get; set; }
		public string taxNumber3Type { get; set; }
		public string taxNumber4 { get; set; }
		public string taxNumber4Type { get; set; }
		public string taxNumber5 { get; set; }
		public string taxNumber5Type { get; set; }
		public string vatNumber { get; set; }
		public string vatNumberType { get; set; }
	}
}