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

using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Logging.MaskingOperator;

public class SecretOperator : RegexMaskingOperator
{
    private const string SecretPattern = "(secret|password)=(.*?)&";

    public SecretOperator()
        : base(SecretPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
    {
    }

    protected override string PreprocessMask(string mask, Match match) => $"{match.Value.Split("=")[0]}{mask}&";

    protected override bool ShouldMaskInput(string input) => input.Contains("secret=") || input.Contains("password=");
}
