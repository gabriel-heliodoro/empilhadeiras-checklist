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
        var baseLogin = $"{NormalizeLoginPart(name)}{NormalizeLoginPart(lastName)}";
        if (string.IsNullOrWhiteSpace(baseLogin))
        {
            baseLogin = "Supervisor";
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

    private static string NormalizeLoginPart(string value)
    {
        var normalized = SupervisorLoginNormalizer.Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        return normalized.Length == 1
            ? normalized.ToUpperInvariant()
            : char.ToUpperInvariant(normalized[0]) + normalized[1..];
    }
}
