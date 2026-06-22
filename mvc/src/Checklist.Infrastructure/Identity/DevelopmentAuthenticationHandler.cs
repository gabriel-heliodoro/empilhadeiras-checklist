using System.Security.Claims;
using System.Text.Encodings.Web;
using Checklist.Application.Common;
using Checklist.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Identity;

public class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevelopmentStub";

    private readonly MvcAuthenticationOptions _authenticationOptions;

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<MvcAuthenticationOptions> authenticationOptions)
        : base(options, logger, encoder)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _authenticationOptions.DevelopmentUserName)
        };

        if (Guid.TryParse(_authenticationOptions.DevelopmentUserId, out var userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            claims.Add(new Claim(CurrentUserClaimTypes.SupervisorId, userId.ToString()));
        }

        if (Guid.TryParse(_authenticationOptions.DevelopmentSectorId, out var setorId))
        {
            claims.Add(new Claim(CurrentUserClaimTypes.SectorId, setorId.ToString()));
        }

        claims.Add(new Claim(CurrentUserClaimTypes.ForceChangePassword, _authenticationOptions.DevelopmentForceChangePassword.ToString().ToLowerInvariant()));
        claims.Add(new Claim(CurrentUserClaimTypes.IsMaster, _authenticationOptions.DevelopmentIsMaster.ToString().ToLowerInvariant()));
        claims.Add(new Claim(CurrentUserClaimTypes.UserType, _authenticationOptions.DevelopmentUserType));

        foreach (var moduleCode in _authenticationOptions.DevelopmentModuleCodes
                     .Where(code => !string.IsNullOrWhiteSpace(code))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(CurrentUserClaimTypes.AccessModule, moduleCode));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
