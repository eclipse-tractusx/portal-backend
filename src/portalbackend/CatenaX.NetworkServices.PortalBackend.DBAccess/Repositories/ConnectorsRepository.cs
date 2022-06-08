using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IConnectorsRepository"/> accessing database with EF Core.
public class ConnectorsRepository : IConnectorsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public ConnectorsRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    ~ConnectorsRepository()
    {
        _context.Database.CurrentTransaction?.Rollback();
        _context.Database.CurrentTransaction?.Dispose();
    }

    /// <inheritdoc/>
    public IQueryable<Connector> GetAllCompanyConnectorsForIamUser(string iamUserId)
    {
        return _context.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == iamUserId)
            .SelectMany(u => u.CompanyUser!.Company!.ProvidedConnectors);
    }

    /// <inheritdoc/>
    public async Task<Connector> CreateConnectorAsync(Connector connector)
    {
        try
        {
            var createdConnector = _context.Connectors.Add(connector);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return createdConnector.Entity;
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Provided connector does not respect database constraints.", nameof(connector));
        }
    }

    /// <inheritdoc/>
    public async Task DeleteConnectorAsync(Guid connectorId)
    {
        try
        {
            var connector = new Connector(connectorId, string.Empty, string.Empty, string.Empty);
            _context.Connectors.Attach(connector);
            _context.Connectors.Remove(connector);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ArgumentException("Connector with provided ID does not exist.", nameof(connectorId));
        }
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync()
    {
        if(_context.Database.CurrentTransaction is null)
        {
            await _context.Database.BeginTransactionAsync();
        }
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync()
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await _context.Database.CurrentTransaction.CommitAsync();
        }
    }
    
    /// <inheritdoc/>
    public async Task RollbackTransactionAsync()
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            await _context.Database.CurrentTransaction.RollbackAsync();
        }
    }
}
