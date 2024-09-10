/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;

/// <summary>
/// Provides constant values
/// </summary>
public static class EscapeExtensions
{
    private static readonly Regex _iLikeExpression = new Regex(@"(?=[\%\\_])", RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

    public static string EscapeForILike(this string value) => _iLikeExpression.Replace(value, @"\");
}
