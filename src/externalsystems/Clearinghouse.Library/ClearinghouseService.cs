/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library;

public class ClearinghouseService : IClearinghouseService
{
	private readonly ITokenService _tokenService;
	private readonly ClearinghouseSettings _settings;

	public ClearinghouseService(ITokenService tokenService, IOptions<ClearinghouseSettings> options)
	{
		_tokenService = tokenService;
		_settings = options.Value;
	}

	/// <inheritdoc />
	public async Task TriggerCompanyDataPost(ClearinghouseTransferData data, CancellationToken cancellationToken)
	{
		var httpClient = await _tokenService.GetAuthorizedClient<ClearinghouseService>(_settings, cancellationToken).ConfigureAwait(false);

		await httpClient.PostAsJsonAsync("/api/v1/validation", data, cancellationToken)
			.CatchingIntoServiceExceptionFor("clearinghouse-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
	}
}
