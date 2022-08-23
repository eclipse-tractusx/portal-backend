using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly PortalDbContext _dbContext;

    public InvitationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IAsyncEnumerable<InvitedUserDetail> GetInvitedUserDetailsUntrackedAsync(Guid applicationId) =>
        (from invitation in _dbContext.Invitations
            join invitationStatus in _dbContext.InvitationStatuses on invitation.InvitationStatusId equals invitationStatus.Id
            join companyuser in _dbContext.CompanyUsers on invitation.CompanyUserId equals companyuser.Id
            join iamuser in _dbContext.IamUsers on companyuser.Id equals iamuser.CompanyUserId
            where invitation.CompanyApplicationId == applicationId
            select new InvitedUserDetail(
                iamuser.UserEntityId,
                invitationStatus.Id,
                companyuser.Email
            ))
        .AsNoTracking()
        .AsAsyncEnumerable();

    public Task<Invitation?> GetInvitationStatusAsync(string iamUserId) =>
        _dbContext.Invitations
            .Where(invitation => invitation.CompanyUser!.IamUser!.UserEntityId == iamUserId)
            .SingleOrDefaultAsync();
}