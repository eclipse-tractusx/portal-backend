using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Extensions;

public static class SeedingExtensions
{
    #region Company

    public static bool UpdateCompanyNeeded(this (Company dataEntity, Company dbEntity) data) =>
        (data.dbEntity.SelfDescriptionDocumentId == null &&
         data.dataEntity.SelfDescriptionDocumentId != data.dbEntity.SelfDescriptionDocumentId) ||
        data.dbEntity.BusinessPartnerNumber != data.dataEntity.BusinessPartnerNumber ||
        data.dbEntity.Shortname != data.dataEntity.Shortname ||
        data.dbEntity.Name != data.dataEntity.Name;

    public static void UpdateCompany(this Company dbEntry, Company entry)
    {
        if (dbEntry.SelfDescriptionDocumentId == null &&
            entry.SelfDescriptionDocumentId != dbEntry.SelfDescriptionDocumentId)
        {
            dbEntry.SelfDescriptionDocumentId = entry.SelfDescriptionDocumentId;
        }

        if (entry.BusinessPartnerNumber != dbEntry.BusinessPartnerNumber)
        {
            dbEntry.BusinessPartnerNumber = entry.BusinessPartnerNumber;
        }

        if (entry.Shortname != dbEntry.Shortname)
        {
            dbEntry.Shortname = entry.Shortname;
        }

        if (entry.Name != dbEntry.Name)
        {
            dbEntry.Name = entry.Name;
        }
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
        if (entry.City != dbEntry.City)
        {
            dbEntry.City = entry.City;
        }

        if (entry.Region != dbEntry.Region)
        {
            dbEntry.Region = entry.Region;
        }

        if (entry.Streetadditional != dbEntry.Streetadditional)
        {
            dbEntry.Streetadditional = entry.Streetadditional;
        }

        if (entry.Streetname != dbEntry.Streetname)
        {
            dbEntry.Streetname = entry.Streetname;
        }

        if (entry.Streetnumber != dbEntry.Streetnumber)
        {
            dbEntry.Streetnumber = entry.Streetnumber;
        }

        if (entry.Zipcode != dbEntry.Zipcode)
        {
            dbEntry.Zipcode = entry.Zipcode;
        }

        if (entry.CountryAlpha2Code != dbEntry.CountryAlpha2Code)
        {
            dbEntry.CountryAlpha2Code = entry.CountryAlpha2Code;
        }
    }

    #endregion
}
