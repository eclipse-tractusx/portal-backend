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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit;
namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests
{
	public class PortalBackendDBAccessTest
	{
		private readonly PortalDbContext mockContext;
		public PortalBackendDBAccessTest()
		{
			mockContext = InMemoryDbContextFactory.GetPortalDbContext();
		}
		[Fact]
		public void GetInvitedUser_Details_by_id()
		{
			//Arrange
			var id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
			mockContext.Invitations.Add(
				new Invitation
				(
					id: new Guid("bd0d0302-3ec8-4bfe-99db-b89bdb6c4b94"),
					companyApplicationId: new Guid("4f0146c6-32aa-4bb1-b844-df7e8babdcb6"),
					companyUserId: new Guid("ac1cf001-7fbc-1f2f-817f-bce0575a0011"),
					invitationStatusId: PortalEntities.Enums.InvitationStatusId.CREATED,
					dateCreated: DateTime.UtcNow
				));
			mockContext.InvitationStatuses.Add(
				new InvitationStatus
				(
					invitationStatusId: PortalEntities.Enums.InvitationStatusId.CREATED
				)
				);
			mockContext.CompanyUsers.Add(
				new CompanyUser
				(
					id: new Guid("ac1cf001-7fbc-1f2f-817f-bce0575a0011"),
					companyId: new Guid("220330ac-170d-4e22-8d72-9467ed042149"),
					companyUserStatusId: PortalEntities.Enums.CompanyUserStatusId.ACTIVE,
					dateCreated: DateTime.UtcNow,
					lastEditorId: new Guid("51F38065-7DB4-43C8-9217-127E88DE1E3C")
				)
				);
			mockContext.IamUsers.Add(
				new IamUser
				(
					iamUserId: "ad56702b-5908-44eb-a668-9a11a0e100d6",
					companyUserId: new Guid("ac1cf001-7fbc-1f2f-817f-bce0575a0011")
				)
				);
			mockContext.SaveChanges();

			var backendDBAccess = new InvitationRepository(mockContext);
			//Act
			var results = backendDBAccess.GetInvitedUserDetailsUntrackedAsync(id);

			//Assert
			Assert.NotNull(results);
		}
	}
}
