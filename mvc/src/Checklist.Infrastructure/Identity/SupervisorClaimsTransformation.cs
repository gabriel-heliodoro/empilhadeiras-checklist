using System.Security.Claims;
using Checklist.Application.Common;
using Checklist.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

public class SupervisorClaimsTransformation : IClaimsTransformation
{
    private readonly AppDbContext _db;

    public SupervisorClaimsTransformation(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var identity = principal.Identities.FirstOrDefault();
        if (identity is null)
        {
            return principal;
        }

        RemoveExistingClaims(identity);

        var candidates = WindowsUserLoginNormalizer.BuildCandidates(identity.Name);
        if (candidates.Count == 0)
        {
            return principal;
        }

        var supervisor = await _db.SupervisorUsers
            .AsNoTracking()
            .Include(x => x.Modules)
            .FirstOrDefaultAsync(x => x.IsActive && candidates.Contains(x.Login));

        if (supervisor is null)
        {
            return principal;
        }

        identity.AddClaim(new Claim(CurrentUserClaimTypes.SupervisorId, supervisor.Id.ToString()));
        identity.AddClaim(new Claim(CurrentUserClaimTypes.SectorId, supervisor.SectorId.ToString()));
        identity.AddClaim(new Claim(CurrentUserClaimTypes.ForceChangePassword, supervisor.ForceChangePassword.ToString().ToLowerInvariant()));
        identity.AddClaim(new Claim(CurrentUserClaimTypes.IsMaster, supervisor.IsMaster.ToString().ToLowerInvariant()));
        identity.AddClaim(new Claim(CurrentUserClaimTypes.UserType, supervisor.UserType.ToString()));

        foreach (var moduleCode in supervisor.Modules.Select(x => AccessModuleMapper.ToCode(x.Module)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(CurrentUserClaimTypes.AccessModule, moduleCode));
        }

        return principal;
    }

    private static void RemoveExistingClaims(ClaimsIdentity identity)
    {
        var customClaims = identity.Claims
            .Where(claim =>
                claim.Type == CurrentUserClaimTypes.SupervisorId ||
                claim.Type == CurrentUserClaimTypes.SectorId ||
                claim.Type == CurrentUserClaimTypes.ForceChangePassword ||
                claim.Type == CurrentUserClaimTypes.IsMaster ||
                claim.Type == CurrentUserClaimTypes.UserType ||
                claim.Type == CurrentUserClaimTypes.AccessModule)
            .ToList();

        foreach (var claim in customClaims)
        {
            identity.RemoveClaim(claim);
        }
    }
}
