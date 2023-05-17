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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public static class DocumentExtensions
{
	public static async Task<(byte[] Content, byte[] Hash)> GetContentAndHash(this IFormFile document, CancellationToken cancellationToken)
	{
		using var sha512 = SHA512.Create();
		using var ms = new MemoryStream((int)document.Length);

		await document.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
		var hash = sha512.ComputeHash(ms);
		var documentContent = ms.GetBuffer();
		if (ms.Length != document.Length || documentContent.Length != document.Length)
		{
			throw new ControllerArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
		}
		return (documentContent, hash);
	}
}
