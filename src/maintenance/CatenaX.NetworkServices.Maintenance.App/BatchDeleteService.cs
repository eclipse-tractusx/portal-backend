using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.Maintenance.App
{
    /// <summary>
    /// Service to delete the pending and inactive documents from the database
    /// </summary>
    public static class BatchDeleteService
    {
        /// <summary>
        /// Deletes the inactive and pending documents that are older than the specified days
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="days"></param>
        /// <exception cref="Exception"></exception>
        public static void BatchDeleteDocuments(PortalDbContext dbContext, int days)
        {
            try
            {
                dbContext.Database.ExecuteSqlRaw(
                    $"delete from portal.documents where date_created < now() - interval '{days} days' and (document_status_id = 1 or document_status_id = 3)");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}