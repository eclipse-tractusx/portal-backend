using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

try
{
    Console.WriteLine("Creating database context");
    using var context = new PortalDbContextFactory().CreateDbContext(Array.Empty<string>());
    Console.WriteLine("Updating database to latest migration");
    await context.Database.MigrateAsync();
    Console.WriteLine("Database was successfully updated to latest migration");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

