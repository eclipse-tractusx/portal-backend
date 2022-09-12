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

﻿using System.Collections.Generic;
using System.Linq;

namespace Keycloak.Net.Common.Extensions
{
    public static class DynamicExtensions
    {
        public static IDictionary<string, object> DynamicToDictionary(dynamic obj) => new Dictionary<string, object>(obj);

        private static string GetFirstPropertyName(IDictionary<string, object> map) => map.Keys.FirstOrDefault();

        public static object GetFirstPropertyValue(dynamic obj)
        {
            var map = DynamicToDictionary(obj);
            return map[GetFirstPropertyName(map)];
        }
    }
}
