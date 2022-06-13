﻿using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IUserBusinessLogic
{
    IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList, string createdByName);
    IAsyncEnumerable<CompanyUserData> GetOwnCompanyUserDatasAsync(string adminUserId, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null, CompanyUserStatusId? status = null);
    IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId,string? languageShortName = null);
    Task<CompanyUserDetails> GetOwnCompanyUserDetails(Guid companyUserId, string iamUserId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid companyUserId, IEnumerable<string> businessPartnerNumbers, string adminUserId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId);
    Task<CompanyUserDetails> GetOwnUserDetails(string iamUserId);
    Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails, string iamUserId);
    Task<int> DeleteOwnUserAsync(Guid companyUserId, string iamUser);
    IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, string adminUserId);
    Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn>? userToUpdateWithBpn);
    Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId, string adminUserId);
}
