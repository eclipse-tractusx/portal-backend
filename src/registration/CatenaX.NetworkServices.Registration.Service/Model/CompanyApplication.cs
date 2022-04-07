using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class CompanyApplication
    {
        public Guid ApplicationId { get; set; }

         [JsonConverter(typeof(StringEnumConverter))]
         public ApplicationStatus ApplicationStatus { get; set; }
    }

    public enum ApplicationStatus
    {
        ADD_COMPANY_DATA = 1,
        INVITE_USER = 2,
        SELECT_COMPANY_ROLE = 3,
        UPLOAD_DOCUMENTS = 4,
        VERIFY = 5,
        SUBMITTED = 6
    }
}
