using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.UserAdministration.Service.Models;

namespace CatenaX.NetworkServices.UserAdministration.Service.BusinessLogic
{
    public interface IUserAdministrationBusinessLogic
    {
        Task<bool> ExecuteInvitation(InvitationData invitationData);
        Task<IEnumerable<string>> CreateUsersAsync(IEnumerable<UserCreationInfo> userList, string tenant, string createdByName);
        Task<IEnumerable<JoinedUserInfo>> GetUsersAsync(
                string tenant,
                string userId = null,
                string providerUserId = null,
                string userName = null,
                string firstName = null,
                string lastName = null,
                string email = null);
        Task<IEnumerable<string>> GetAppRolesAsync(string clientId);
        Task<bool> DeleteUserAsync(string tenant, string userId);
        Task<IEnumerable<string>> DeleteUsersAsync(UserIds userList, string tenant);
        Task<bool> AddBpnAttributeAtRegistrationApprovalAsync(Guid companyId);
        Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn> userToUpdateWithBpn);
    }
}
