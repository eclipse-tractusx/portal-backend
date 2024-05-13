/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public static class NameHelper
{
    public static string CreateNameString(string? firstName, string? lastName, string? email, string fallback)
    {
        var sb = new StringBuilder();
        if (firstName != null)
        {
            sb.Append(firstName);
        }

        if (lastName != null)
        {
            sb.AppendFormat(sb.Length == 0 ? "{0}" : ", {0}", lastName);
        }

        if (email != null)
        {
            sb.AppendFormat(sb.Length == 0 ? "{0}" : " ({0})", email);
        }

        return sb.Length == 0 ? fallback : sb.ToString();
    }
}
