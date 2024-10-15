using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Extensions;

public static class SeedingExtensions
{
    #region Company

    public static bool UpdateCompanyNeeded(this (Company dataEntity, Company dbEntity) data) =>
        (data.dbEntity.SelfDescriptionDocumentId == null &&
         data.dataEntity.SelfDescriptionDocumentId != null) ||
        data.dbEntity.BusinessPartnerNumber != data.dataEntity.BusinessPartnerNumber ||
        data.dbEntity.Shortname != data.dataEntity.Shortname ||
        data.dbEntity.Name != data.dataEntity.Name;

    public static void UpdateCompany(this Company dbEntry, Company entry)
    {
        if (dbEntry.SelfDescriptionDocumentId == null &&
            entry.SelfDescriptionDocumentId != null)
        {
            dbEntry.SelfDescriptionDocumentId = entry.SelfDescriptionDocumentId;
        }

        dbEntry.BusinessPartnerNumber = entry.BusinessPartnerNumber;
        dbEntry.Shortname = entry.Shortname;
        dbEntry.Name = entry.Name;
    }

    #endregion

    #region Address

    public static bool UpdateAddressNeeded(this (Address dataEntity, Address dbEntity) data) =>
        data.dataEntity.City != data.dbEntity.City ||
        data.dbEntity.Region != data.dataEntity.Region ||
        data.dbEntity.Streetadditional != data.dataEntity.Streetadditional ||
        data.dbEntity.Streetname != data.dataEntity.Streetname ||
        data.dbEntity.Streetnumber != data.dataEntity.Streetnumber ||
        data.dbEntity.Zipcode != data.dataEntity.Zipcode ||
        data.dbEntity.CountryAlpha2Code != data.dataEntity.CountryAlpha2Code;

    public static void UpdateAddress(this Address dbEntry, Address entry)
    {
        dbEntry.City = entry.City;
        dbEntry.Region = entry.Region;
        dbEntry.Streetadditional = entry.Streetadditional;
        dbEntry.Streetname = entry.Streetname;
        dbEntry.Streetnumber = entry.Streetnumber;
        dbEntry.Zipcode = entry.Zipcode;
        dbEntry.CountryAlpha2Code = entry.CountryAlpha2Code;
    }

    #endregion
}
