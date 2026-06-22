using Checklist.Application.Common;
using Checklist.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Checklist.Infrastructure.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var rawId = _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentUserClaimTypes.SupervisorId)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawId, out var userId) ? userId : null;
        }
    }

    public Guid? SectorId
    {
        get
        {
            var rawSectorId = _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentUserClaimTypes.SectorId);
            return Guid.TryParse(rawSectorId, out var setorId) ? setorId : null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User.Identity?.Name
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool ForceChangePassword =>
        bool.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentUserClaimTypes.ForceChangePassword), out var forceChangePassword)
        && forceChangePassword;

    public bool IsMaster =>
        bool.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentUserClaimTypes.IsMaster), out var isMaster)
        && isMaster;

    public string? UserType =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentUserClaimTypes.UserType);

    public IReadOnlyCollection<string> ModuleCodes =>
        _httpContextAccessor.HttpContext?.User.FindAll(CurrentUserClaimTypes.AccessModule)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? [];
}
