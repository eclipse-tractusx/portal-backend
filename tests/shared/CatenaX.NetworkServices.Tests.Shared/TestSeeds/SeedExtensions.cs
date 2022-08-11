using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
namespace CatenaX.NetworkServices.Tests.Shared.TestSeeds;

public static class SeedExtensions
{
    public static Action<PortalDbContext> SeedAddress(params Address[] additionalAddresses) => dbContext =>
        {
            dbContext.Addresses.AddRange(additionalAddresses);
        };
    
    public static Action<PortalDbContext> SeedCompany(params Company[] additionalCompanies) => dbContext =>
        {
            dbContext.Companies.AddRange(additionalCompanies);
        };
    
    public static Action<PortalDbContext> SeedCompanyUser(params CompanyUser[] additionalUsers) => dbContext =>
        {
            dbContext.CompanyUsers.AddRange(additionalUsers);
        };
        
    public static Action<PortalDbContext> SeedIamUsers(params IamUser[] additionalUsers) => dbContext =>
        {
            dbContext.IamUsers.AddRange(additionalUsers);
        };

    public static Action<PortalDbContext> SeedIamClients(params IamClient[] additionalIamClients) =>
        dbContext =>
        {
            dbContext.IamClients.AddRange(additionalIamClients);
        };

    public static Action<PortalDbContext> SeedUserRoles(
        params UserRole[] additionalCompanyUserRoles) =>
        dbContext =>
        {
            dbContext.UserRoles.AddRange(additionalCompanyUserRoles);
        };
    
    public static Action<PortalDbContext> SeedCompanyUserAssignedRoles(
        params CompanyUserAssignedRole[] additionalCompanyUserRoles) =>
        dbContext =>
        {
            dbContext.CompanyUserAssignedRoles.AddRange(additionalCompanyUserRoles);
        };

    public static Action<PortalDbContext> SeedNotification(params Notification[] notifications) => dbContext =>
    {
        dbContext.Notifications.AddRange(notifications);
    };
}