using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

public class AddressSeeder : ICustomSeeder
{
    private readonly IHostEnvironment _environment;
    private readonly PortalDbContext _context;
    private readonly ILogger<AddressSeeder> _logger;

    public AddressSeeder(IHostEnvironment environment, PortalDbContext context, ILogger<AddressSeeder> logger)
    {
        _environment = environment;
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!await _context.Addresses.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Started to Seed Addresses");
            
            var addressData = await File.ReadAllTextAsync(path + "/Seeder/address.json", cancellationToken);
            var addresses = JsonSerializer.Deserialize<List<Address>>(addressData);

            if (addresses != null)
            {
                foreach (var address in addresses)
                {
                    await _context.Addresses.AddAsync(address, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded Addresses");
        }
    }
}