using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class IdentityProviderBusinessLogic : IIdentityProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    public IdentityProviderBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public async IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId)
    {
        await foreach ( var identityProviderData in _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderCategoryDataUntracked(iamUserId).ConfigureAwait(false))
        {
            switch(identityProviderData.CategoryId)
            {
                case IdentityProviderCategoryId.KEYCLOAK_SHARED:
                case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                    yield return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderData.IdentityProviderId, identityProviderData.Alias, identityProviderData.CategoryId).ConfigureAwait(false);
                    break;
                case IdentityProviderCategoryId.KEYCLOAK_SAML:
                    var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(identityProviderData.Alias).ConfigureAwait(false);
                    yield return new IdentityProviderDetails(
                        identityProviderData.IdentityProviderId,
                        identityProviderData.Alias,
                        identityProviderData.CategoryId,
                        identityProviderDataSAML.DisplayName,
                        identityProviderDataSAML.RedirectUrl,
                        identityProviderDataSAML.Enabled)
                        {
                            saml = new IdentityProviderDetailsSAML(
                                identityProviderDataSAML.EntityId,
                                identityProviderDataSAML.SingleSignOnServiceUrl)
                        };
                    break;
            }
        }
    }

    public async Task<IdentityProviderDetails> CreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol, string iamUserId)
    {
        IdentityProviderCategoryId identityProviderCategory = default!;
        switch (protocol)
        {
            case IamIdentityProviderProtocol.SAML:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_SAML;
                break;
            case IamIdentityProviderProtocol.OIDC:
                identityProviderCategory = IdentityProviderCategoryId.KEYCLOAK_OIDC;
                break;
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
        }

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var (companyName, companyId) = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameIdUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (companyName == null || companyId == default)
        {
            throw new Exception($"user {iamUserId} is not associated with a company");
        }
        var alias = await _provisioningManager.CreateOwnIdpAsync(companyName, protocol).ConfigureAwait(false);
        var identityProvider = identityProviderRepository.CreateIdentityProvider(identityProviderCategory);
        identityProvider.CompanyIdentityProviders.Add(identityProviderRepository.CreateCompanyIdentityProvider(companyId, identityProvider.Id));
        var iamIdentityProvider = identityProviderRepository.CreateIamIdentityProvider(identityProvider, alias);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        switch (protocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProvider.Id, alias, IdentityProviderCategoryId.KEYCLOAK_OIDC).ConfigureAwait(false);
            case IamIdentityProviderProtocol.SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProvider.Id, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"unexcepted value of protocol: {protocol.ToString()}", nameof(protocol));
        }
    }

    public async Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId)
    {
        var (alias, category, isOwnCompany) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (!isOwnCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"identityProvider {identityProviderId} category cannot be updated");
        }
    }

    public async Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider(Guid identityProviderId, IdentityProviderEditableDetails details, string iamUserId)
    {
        var (alias, category, isOwnCompany) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (!isOwnCompany)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        if (alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        switch(category)
        {
            case IdentityProviderCategoryId.KEYCLOAK_OIDC:
                if(details.oidc == null)
                {
                    throw new ArgumentNullException("property 'oidc' must not be null", nameof(details));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(
                    alias,
                    details.displayName,
                    details.enabled,
                    details.oidc.authorizationUrl,
                    details.oidc.clientAuthMethod,
                    details.oidc.clientId,
                    details.oidc.secret,
                    details.oidc.signatureAlgorithm)
                    .ConfigureAwait(false);
                var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsOIDCAsync(identityProviderId, alias, category).ConfigureAwait(false);
            case IdentityProviderCategoryId.KEYCLOAK_SAML:
                if(details.saml == null)
                {
                    throw new ArgumentNullException("property 'saml' must not be null", nameof(details));
                }
                await _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(
                    alias,
                    details.displayName,
                    details.enabled,
                    details.saml.serviceProviderEntityId,
                    details.saml.singleSignOnServiceUrl)
                    .ConfigureAwait(false);
                var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
                return await GetCentralIdentityProviderDetailsSAMLAsync(identityProviderId, alias).ConfigureAwait(false);
            default:
                throw new ArgumentException($"identityProvider {identityProviderId} category {category} cannot be updated");
        }
    }

    public async Task DeleteOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} does not exist");
        }
        var (companyId, alias, companyCount) = result;
        if (companyId == default)
        {
            throw new ForbiddenException($"identityProvider {identityProviderId} is not associated with company of user {iamUserId}");
        }
        _portalRepositories.Remove(new CompanyIdentityProvider(companyId, identityProviderId));
        if (companyCount == 1)
        {
            if (alias != null)
            {
                _portalRepositories.Remove(new IamIdentityProvider(alias, default!));
                await _provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(false);
            }
            _portalRepositories.Remove(_portalRepositories.Attach(new IdentityProvider(identityProviderId, default, default)));
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task<IdentityProviderDetails> GetCentralIdentityProviderDetailsOIDCAsync(Guid identityProviderId, string alias, IdentityProviderCategoryId categoryId)
    {
        var identityProviderDataOIDC = await _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            categoryId,
            identityProviderDataOIDC.DisplayName,
            identityProviderDataOIDC.RedirectUrl,
            identityProviderDataOIDC.Enabled)
            {
                oidc = new IdentityProviderDetailsOIDC(
                    identityProviderDataOIDC.AuthorizationUrl,
                    identityProviderDataOIDC.ClientId,
                    identityProviderDataOIDC.ClientAuthMethod)
                    {
                        signatureAlgorithm = identityProviderDataOIDC.SignatureAlgorithm
                    }
            };
    }
    private async Task<IdentityProviderDetails> GetCentralIdentityProviderDetailsSAMLAsync(Guid identityProviderId, string alias)
    {
        var identityProviderDataSAML = await _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(alias).ConfigureAwait(false);
        return new IdentityProviderDetails(
            identityProviderId,
            alias,
            IdentityProviderCategoryId.KEYCLOAK_SAML,
            identityProviderDataSAML.DisplayName,
            identityProviderDataSAML.RedirectUrl,
            identityProviderDataSAML.Enabled)
            {
                saml = new IdentityProviderDetailsSAML(
                    identityProviderDataSAML.EntityId,
                    identityProviderDataSAML.SingleSignOnServiceUrl)
            };
    }


    public async Task<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderLinkData.identityProviderId, iamUserId).ConfigureAwait(false);

        try
        {
            await _provisioningManager.AddProviderUserLinkToCentralUserAsync(userEntityId, alias, identityProviderLinkData.userId, identityProviderLinkData.userName).ConfigureAwait(false);
        }
        catch(KeycloakEntityConflictException ce)
        {
            throw new ConflictException($"identityProviderLink for identityProvider {identityProviderLinkData.identityProviderId} already exists for user {companyUserId}", ce);
        }
        
        return new UserIdentityProviderLinkData(
            identityProviderLinkData.identityProviderId,
            identityProviderLinkData.userId,
            identityProviderLinkData.userName);
    }

    public async Task<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);

        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias);
        }
        catch(KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
        await _provisioningManager.AddProviderUserLinkToCentralUserAsync(userEntityId, alias, userLinkData.userId, userLinkData.userName).ConfigureAwait(false);
        
        return new UserIdentityProviderLinkData(
            identityProviderId,
            userLinkData.userId,
            userLinkData.userName);
    }

    public async Task<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);

        var result = (await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(Enumerable.Repeat(alias,1), userEntityId).ConfigureAwait(false)).SingleOrDefault();
        if (result == default)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");
        }
        return new UserIdentityProviderLinkData(
            identityProviderId,
            result.UserId,
            result.UserName);
    }

    public async Task DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var (userEntityId, alias) = await GetUserAliasDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
        try
        {
            await _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(userEntityId, alias).ConfigureAwait(false);
        }
        catch(KeycloakEntityNotFoundException e)
        {
            throw new NotFoundException($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}", e);
        }
    }

    public async IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);

        await foreach (var data in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId, idpAliasDatas).ConfigureAwait(false))
        {
            yield return new UserIdentityProviderData(
                data.CompanyUserId,
                data.FirstName,
                data.LastName,
                data.Email,
                data.LinkDatas.Select(linkData => new UserIdentityProviderLinkData(
                    linkData.IdentityProviderId,
                    linkData.UserId,
                    linkData.UserName))
            );
        }
    }

    public Stream GetOwnCompanyUsersIdentityProviderDataStream(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        return new AsyncEnumerableStringStream(GetOwnCompanyUsersIdentityProviderDataLines(identityProviderIds, iamUserId), Encoding.ASCII);
    }

    private async IAsyncEnumerable<string> GetOwnCompanyUsersIdentityProviderDataLines(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        var idpAliasDatas = await GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(identityProviderIds, iamUserId).ConfigureAwait(false);
        var idpIds = idpAliasDatas.Select(data => data.IdentityProviderId).ToList();

        bool firstLine = true;

        await foreach (var data in GetOwnCompanyIdentityProviderLinkDataInternalAsync(iamUserId, idpAliasDatas).ConfigureAwait(false))
        {
            if (firstLine)
            {
                firstLine = false;
                yield return string.Join(
                    ",",
                    "Id",
                    "FirstName",
                    "LastName",
                    "Email",
                    string.Join(",", idpAliasDatas.SelectMany(data => new [] { $"UserId {data.Alias}", $"UserName {data.Alias}" }))
                );
            }
            yield return string.Join(
                ",",
                data.CompanyUserId,
                data.FirstName,
                data.LastName,
                data.Email,
                string.Join(",", idpIds.SelectMany(identityProviderId =>
                    {
                        var linkData = data.LinkDatas.SingleOrDefault(linkData => linkData.IdentityProviderId == identityProviderId);
                        return new [] { linkData.UserId, linkData.UserName };
                    })));
        }
    }

    private async Task<IEnumerable<(Guid IdentityProviderId, string Alias)>> GetOwnCompanyUsersIdentityProviderAliasDataInternalAsync(IEnumerable<Guid> identityProviderIds, string iamUserId)
    {
        if (!identityProviderIds.Any())
        {
            throw new ControllerArgumentException("at lease one identityProviderId must be specified", nameof(identityProviderIds));
        }
        var identityProviderData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetOwnCompanyIdentityProviderAliasDataUntracked(iamUserId, identityProviderIds).ToListAsync().ConfigureAwait(false);

        var invalidIds = identityProviderIds.Except(identityProviderData.Select(data => data.IdentityProviderId));
        if (invalidIds.Any())
        {
            throw new ControllerArgumentException($"invalid identityProviders: [{String.Join(", ", invalidIds)}] for user {iamUserId}", nameof(identityProviderIds));
        }

        return identityProviderData;
    }

    private async IAsyncEnumerable<(Guid CompanyUserId, string? FirstName, string? LastName, string? Email, IEnumerable<(Guid IdentityProviderId, string UserId, string UserName)> LinkDatas)> GetOwnCompanyIdentityProviderLinkDataInternalAsync(string iamUserId, IEnumerable<(Guid IdentityProviderId, string Alias)> identityProviderAliasDatas)
    {
        var idPerAlias = identityProviderAliasDatas.ToDictionary(item => item.Alias, item => item.IdentityProviderId);
        var aliase = identityProviderAliasDatas.Select(item => item.Alias).ToList();

        await foreach(var (companyUserId, firstName, lastName, email, userEntityId) in _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyUserQuery(iamUserId)
            .Select(companyUser =>
                ((Guid, string?, string?, string?, string?)) new (
                    companyUser.Id,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Email,
                    companyUser.IamUser!.UserEntityId))
            .ToAsyncEnumerable().ConfigureAwait(false))
        {
            if (userEntityId != null)
            {
                var linkDatas = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(aliase, userEntityId).ConfigureAwait(false);
                yield return (companyUserId, firstName, lastName, email, linkDatas.Select(linkData => (idPerAlias[linkData.Alias], linkData.UserId, linkData.UserName)));
            }
        }
    }

    private class AsyncEnumerableStringStream : Stream
    {
        public AsyncEnumerableStringStream(IAsyncEnumerable<string> data, Encoding encoding) : base()
        {
            _enumerator = data.GetAsyncEnumerator();
            _stream = new MemoryStream();
            _writer = new StreamWriter(_stream, encoding);
        }

        private readonly IAsyncEnumerator<string> _enumerator;
        private readonly MemoryStream _stream;
        private readonly TextWriter _writer;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override long Seek (long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte [] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Write(byte [] buffer, int offset, int count) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            var position = offset;
            var remaining = count;
            var written = _stream.Read(buffer, position, remaining);
            remaining = remaining - written;
            while (remaining > 0 && await _enumerator.MoveNextAsync(token).ConfigureAwait(false))
            {
                _stream.Position = 0;
                _stream.SetLength(0);
                _writer.WriteLine(_enumerator.Current);
                _writer.Flush();
                _stream.Position = 0;

                position = position + written;
                written = _stream.Read(buffer, position, remaining);
                remaining = remaining - written;
            }
            return count - remaining;
        }
    }

    private async Task<(string UserEntityId, string Alias)> GetUserAliasDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId)
    {
        var userAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
        if (userAliasData == default)
        {
            throw new NotFoundException($"companyUserId {companyUserId} does not exist");
        }
        if (userAliasData.UserEntityId == null)
        {
            throw new UnexpectedConditionException($"companyUserId {companyUserId} is not linked to keycloak");
        }
        if (userAliasData.Alias == null)
        {
            throw new NotFoundException($"identityProvider {identityProviderId} not found in company of user {companyUserId}" );
        }
        if (!userAliasData.IsSameCompany)
        {
            throw new ForbiddenException($"user {iamUserId} does not belong to company of companyUserId {companyUserId}");
        }
        return ((string UserEntityId, string Alias)) new (
            userAliasData.UserEntityId,
            userAliasData.Alias);
    }
}
