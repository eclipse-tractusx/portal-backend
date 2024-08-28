using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Extensions;

public static class CompanyApplicationStatusFilterExtensions
{
    public static IEnumerable<CompanyApplicationStatusId> GetCompanyApplicationStatusIds(this CompanyApplicationStatusFilter? companyApplicationStatusFilter) =>
        companyApplicationStatusFilter switch
        {
            CompanyApplicationStatusFilter.Closed =>
            [
                CompanyApplicationStatusId.CONFIRMED, CompanyApplicationStatusId.DECLINED
            ],
            CompanyApplicationStatusFilter.InReview => [CompanyApplicationStatusId.SUBMITTED],
            _ =>
            [
                CompanyApplicationStatusId.SUBMITTED, CompanyApplicationStatusId.CONFIRMED,
                CompanyApplicationStatusId.DECLINED
            ]
        };
}
