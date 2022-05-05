using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests
{
    public class InMemoryDbContextFactory
    {
        public PortalDbContext GetPortalDbContext()
        {
            var options = new DbContextOptionsBuilder<PortalDbContext>()
                               .UseInMemoryDatabase(databaseName: "InMemoryPortalDatabase").Options;
            var dbContext = new PortalDbContext(options);
            return dbContext;
        }
    }
}
