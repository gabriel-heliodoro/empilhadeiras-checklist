using Checklist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

public class SupervisorLoginGenerator
{
    private readonly AppDbContext _dbContext;

    public SupervisorLoginGenerator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateUniqueLoginAsync(
        string name,
        string lastName,
        Guid? excludeSupervisorId = null,
        CancellationToken cancellationToken = default)
    {
        var baseLogin = SupervisorLoginNormalizer.Normalize($"{name}{lastName}");
        if (string.IsNullOrWhiteSpace(baseLogin))
        {
            baseLogin = "supervisor";
        }

        var candidate = baseLogin;
        var suffix = 2;

        while (await _dbContext.SupervisorUsers.AnyAsync(
                   user => user.Login == candidate && (!excludeSupervisorId.HasValue || user.Id != excludeSupervisorId.Value),
                   cancellationToken))
        {
            candidate = $"{baseLogin}{suffix}";
            suffix++;
        }

        return candidate;
    }
}
