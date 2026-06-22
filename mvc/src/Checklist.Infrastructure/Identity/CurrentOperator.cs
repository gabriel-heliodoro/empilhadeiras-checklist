using System.Security.Claims;
using Checklist.Application.Common;
using Checklist.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Checklist.Infrastructure.Identity;

public class CurrentOperator : ICurrentOperator
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentOperator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var rawId = _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentOperatorClaimTypes.OperatorId)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawId, out var operatorId) ? operatorId : null;
        }
    }

    public Guid? SectorId
    {
        get
        {
            var rawId = _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentOperatorClaimTypes.SectorId);
            return Guid.TryParse(rawId, out var setorId) ? setorId : null;
        }
    }

    public string? Name =>
        _httpContextAccessor.HttpContext?.User.Identity?.Name
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? SectorName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentOperatorClaimTypes.SectorName);

    public string? Registration =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentOperatorClaimTypes.Registration);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool ForceChangePassword =>
        bool.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(CurrentOperatorClaimTypes.ForceChangePassword), out var forceChangePassword)
        && forceChangePassword;
}
