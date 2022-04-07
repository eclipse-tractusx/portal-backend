using CatenaX.NetworkServices.Cosent.Library;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CatenaX.NetworkServices.Consent.Service.Tests
{
    public class ConstentAccessTests
    {
        [Fact]
        public void GetSpecificCosents()
        {

            var cs = "";
            var crAccess = new ConsentAccess(cs);

            var consents = crAccess.GetConsents(new[] {1,2});

            Assert.NotNull(consents);
        }

        [Fact]
        public void GetSignedConsents()
        {
            var cs = "";
            var crAccess = new ConsentAccess(cs);

            var consents = crAccess.GetSignedConsentsForCompanyId("test 3");

            Assert.NotNull(consents);
        }

        [Fact]
        public void SignConsent()
        {
            var cs = "";
            var crAccess = new ConsentAccess(cs);

           crAccess.SignConsent("test 2",1,2,"xunit");
        }
    }


}
