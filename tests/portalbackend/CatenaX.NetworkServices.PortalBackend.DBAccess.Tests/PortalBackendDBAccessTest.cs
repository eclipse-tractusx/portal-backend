using System;
using Xunit;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests
{
    public class PortalBackendDBAccessTest
    {
        private PortalDbContext mockContext;
        private IPortalBackendDBAccess backendDBAccess;
        public PortalBackendDBAccessTest()
        {
            mockContext = new InMemoryDbContextFactory().GetPortalDbContext();
        }
        [Fact]
        public void GetInvitedUser_Details_by_id()
        {
            //Arrange
            Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            mockContext.Invitations.Add(
                new Invitation
                (
                    id: new Guid("bd0d0302-3ec8-4bfe-99db-b89bdb6c4b94"),
                    companyApplicationId: new Guid("4f0146c6-32aa-4bb1-b844-df7e8babdcb6"),
                    companyUserId: new Guid("ac1cf001-7fbc-1f2f-817f-bce0575a0011"),
                    invitationStatusId: PortalBackend.PortalEntities.Enums.InvitationStatusId.CREATED,
                    dateCreated: DateTime.UtcNow

                ));
            mockContext.InvitationStatuses.Add(
                new InvitationStatus
                (
                    invitationStatusId: PortalBackend.PortalEntities.Enums.InvitationStatusId.CREATED
                )
                );
            mockContext.CompanyUsers.Add(
                new CompanyUser
                (
                    id: new Guid("ac1cf001-7fbc-1f2f-817f-bce0575a0011"),
                    companyId: new Guid("220330ac-170d-4e22-8d72-9467ed042149"),
                    companyUserStatusId: PortalEntities.Enums.CompanyUserStatusId.ACTIVE,
                    dateCreated: DateTime.UtcNow
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

            backendDBAccess = new PortalBackendDBAccess(mockContext);
            //Act
            var results = backendDBAccess.GetInvitedUserDetailsUntrackedAsync(id);

            //Assert
            Assert.NotNull(results);
        }
    }
}