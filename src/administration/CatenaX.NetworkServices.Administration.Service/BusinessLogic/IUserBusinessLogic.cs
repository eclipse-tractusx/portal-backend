﻿using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public interface IUserBusinessLogic
    {
        IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList, string createdByName);
        IAsyncEnumerable<CompanyUserDetails> GetCompanyUserDetailsAsync(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, CompanyUserStatusId? status = null);
        Task<IEnumerable<string>> GetAppRolesAsync(string? clientId);
        Task<int> DeleteUserAsync(string iamUser);
        IAsyncEnumerable<Guid> DeleteUsersAsync(IEnumerable<Guid> companyUserIds, string adminUserId);
        Task<bool> AddBpnAttributeAtRegistrationApprovalAsync(Guid companyId);
        Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn>? userToUpdateWithBpn);
        Task<bool> PostRegistrationWelcomeEmailAsync(WelcomeData welcomeData);
        Task<bool> ExecutePasswordReset(Guid companyUserId, string adminUserId, string tenant);
    }
}
