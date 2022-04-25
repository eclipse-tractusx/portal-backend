using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class CompanyApplication
    {
        public Guid ApplicationId { get; set; }

         [JsonConverter(typeof(StringEnumConverter))]
         public CompanyApplicationStatusId? ApplicationStatus { get; set; }
    }
}
