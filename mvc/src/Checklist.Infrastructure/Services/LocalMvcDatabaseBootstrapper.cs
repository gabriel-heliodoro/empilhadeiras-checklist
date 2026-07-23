using Checklist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class LocalMvcDatabaseBootstrapper
{
    private readonly AppDbContext _dbContext;

    public LocalMvcDatabaseBootstrapper(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.IsSqlServer())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
    }
}
