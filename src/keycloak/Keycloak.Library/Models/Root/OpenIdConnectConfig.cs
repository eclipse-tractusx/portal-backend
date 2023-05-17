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

using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Root;

public class OpenIdConnectConfig
{
	[JsonProperty("userinfo.token.claim")]
	public bool? UserInfoTokenClaim { get; set; }

	[JsonProperty("user.attribute")]
	public string UserAttribute { get; set; }

	[JsonProperty("id.token.claim")]
	public bool? IdTokenClaim { get; set; }

	[JsonProperty("access.token.claim")]
	public bool? AccessTokenClaim { get; set; }

	[JsonProperty("claim.name")]
	public string ClaimName { get; set; }

	[JsonProperty("jsonType.label")]
	public JsonTypeLabel? JsonTypeLabel { get; set; }

	[JsonProperty("user.attribute.formatted")]
	public string UserAttributeFormatted { get; set; }

	[JsonProperty("user.attribute.country")]
	public string UserAttributeCountry { get; set; }

	[JsonProperty("user.attribute.postal_code")]
	public string UserAttributePostalCode { get; set; }

	[JsonProperty("user.attribute.street")]
	public string UserAttributeStreet { get; set; }

	[JsonProperty("user.attribute.region")]
	public string UserAttributeRegion { get; set; }

	[JsonProperty("user.attribute.locality")]
	public string UserAttributeLocality { get; set; }

	[JsonProperty("user.session.note")]
	public string UserSessionNote { get; set; }

	[JsonProperty("multivalued")]
	public bool? Multivalued { get; set; }
}
