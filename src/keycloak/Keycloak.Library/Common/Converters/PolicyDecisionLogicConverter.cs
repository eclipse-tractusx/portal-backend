/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationPermissions;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Converters;

public class PolicyDecisionLogicConverter : JsonEnumConverter<PolicyDecisionLogic>
{
	private static readonly Dictionary<PolicyDecisionLogic, string> SPairs = new Dictionary<PolicyDecisionLogic, string>
	{
		[PolicyDecisionLogic.Positive] = "POSITIVE",
		[PolicyDecisionLogic.Negative] = "NEGATIVE"
	};

	protected override string EntityString { get; } = "logic";

	protected override string ConvertToString(PolicyDecisionLogic value) => SPairs[value];

	protected override PolicyDecisionLogic ConvertFromString(string s)
	{
		if (SPairs.ContainsValue(s.ToUpper()))
		{
			return SPairs.First(kvp => kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase)).Key;
		}

		throw new ArgumentException($"Unknown {EntityString}: {s}");
	}
}
