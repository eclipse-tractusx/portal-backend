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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

public static class OfferData
{
	public static readonly ImmutableList<Offer> Offers = ImmutableList.Create(
		new Offer(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "Catena X", DateTimeOffset.UtcNow, OfferTypeId.APP)
		{
			DateReleased = DateTimeOffset.UtcNow.AddDays(-1),
			Name = "Top App",
			OfferStatusId = OfferStatusId.ACTIVE,
			ProviderCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
			LicenseTypeId = LicenseTypeId.COTS
		},
		new Offer(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "Catena X", DateTimeOffset.UtcNow, OfferTypeId.SERVICE)
		{
			Name = "Newest Service",
			ContactEmail = "service-test@mail.com",
			OfferStatusId = OfferStatusId.ACTIVE,
			ProviderCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
			DateReleased = DateTimeOffset.UtcNow.Add(new TimeSpan(1, 0, 0)),
			LicenseTypeId = LicenseTypeId.COTS
		},
		new Offer(new Guid("c208a2ac-7577-4a62-9504-300a2007b198"), "Catena Y", DateTimeOffset.UtcNow, OfferTypeId.SERVICE)
		{
			Name = "Newest Service 2",
			ContactEmail = "service-test2@mail.com",
			OfferStatusId = OfferStatusId.ACTIVE,
			ProviderCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"),
			DateReleased = DateTimeOffset.UtcNow.Add(new TimeSpan(2, 0, 0)),
			LicenseTypeId = LicenseTypeId.COTS
		},
		new Offer(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA6"), "Catena X", DateTimeOffset.UtcNow, OfferTypeId.APP)
		{
			Name = "Latest Service",
			ContactEmail = "service-test@mail.com",
			OfferStatusId = OfferStatusId.IN_REVIEW,
			ProviderCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
			LicenseTypeId = LicenseTypeId.COTS
		}
	);
}
