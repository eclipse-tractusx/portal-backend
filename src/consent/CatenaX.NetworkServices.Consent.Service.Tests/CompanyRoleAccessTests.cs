using CatenaX.NetworkServices.Cosent.Library;
using System;
using Xunit;

namespace CatenaX.NetworkServices.Consent.Service.Tests
{
    public class CompanyRoleAccessTests
    {
        [Fact]
        public void GetAllRoles()
        {
            var cs = "";
            var crAccess = new CompanyRoleAccess(cs);

           var roles = crAccess.GetCompanyRoles();

            Assert.NotNull(roles);
        }

        [Fact]
        public void GetAllRolesById()
        {
            var cs = "";
            var crAccess = new CompanyRoleAccess(cs);

            var roles = crAccess.GetCompanyRoles(1);

            Assert.NotNull(roles);
        }
    }
}
