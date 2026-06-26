using System.Security.Claims;
using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Identity;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly ISupervisorAuthenticationService _authenticationService;

    public AccountController(ISupervisorAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl, BuildLandingContext(User));
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.AuthenticateAsync(model.Login, model.Password, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel autenticar o supervisor.");
            return View(model);
        }

        var supervisor = result.Value;
        var claims = BuildClaims(supervisor);
        var identity = new ClaimsIdentity(claims, MvcAuthenticationSchemes.Supervisor);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            MvcAuthenticationSchemes.Supervisor,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        return RedirectToLocal(model.ReturnUrl, BuildLandingContext(supervisor));
    }

    [HttpGet("access-denied")]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(MvcAuthenticationSchemes.Supervisor);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl, LandingContext context)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl)
            && !IsLoopingLoginReturnUrl(returnUrl)
            && !(context.ShouldIgnoreRootReturnUrl && string.Equals(returnUrl, "/", StringComparison.Ordinal)))
        {
            return Redirect(returnUrl);
        }

        if (context.IsMaster)
        {
            return RedirectToAction("Sectors", "Master");
        }

        if (string.Equals(context.UserType, "Supervisor", StringComparison.Ordinal)
            && context.ModuleCodes.Contains(AccessModuleCodes.OperationalSupervision, StringComparer.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (string.Equals(context.UserType, "Inspector", StringComparison.Ordinal)
            && context.ModuleCodes.Contains(AccessModuleCodes.WorkSafety, StringComparer.OrdinalIgnoreCase))
        {
            return RedirectToAction("Dashboard", "Stp");
        }

        return RedirectToAction(nameof(AccessDenied));
    }

    private static bool IsLoopingLoginReturnUrl(string returnUrl)
    {
        return returnUrl.StartsWith("/account/login", StringComparison.OrdinalIgnoreCase);
    }

    private static LandingContext BuildLandingContext(ClaimsPrincipal user)
    {
        return new LandingContext(
            bool.TryParse(user.FindFirst(CurrentUserClaimTypes.IsMaster)?.Value, out var isMaster) && isMaster,
            user.FindFirst(CurrentUserClaimTypes.UserType)?.Value ?? string.Empty,
            user.FindAll(CurrentUserClaimTypes.AccessModule).Select(x => x.Value).ToArray());
    }

    private static LandingContext BuildLandingContext(SupervisorSessionDto supervisor)
    {
        return new LandingContext(
            supervisor.IsMaster,
            supervisor.UserType,
            supervisor.ModuleCodes.ToArray());
    }

    private static IReadOnlyCollection<Claim> BuildClaims(SupervisorSessionDto supervisor)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, supervisor.Id.ToString()),
            new(ClaimTypes.Name, supervisor.DisplayName),
            new(CurrentUserClaimTypes.SupervisorId, supervisor.Id.ToString()),
            new(CurrentUserClaimTypes.SectorId, supervisor.SectorId.ToString()),
            new(CurrentUserClaimTypes.ForceChangePassword, supervisor.ForceChangePassword.ToString().ToLowerInvariant()),
            new(CurrentUserClaimTypes.IsMaster, supervisor.IsMaster.ToString().ToLowerInvariant()),
            new(CurrentUserClaimTypes.UserType, supervisor.UserType)
        };

        foreach (var moduleCode in supervisor.ModuleCodes)
        {
            claims.Add(new Claim(CurrentUserClaimTypes.AccessModule, moduleCode));
        }

        return claims;
    }

    private sealed record LandingContext(
        bool IsMaster,
        string UserType,
        IReadOnlyCollection<string> ModuleCodes)
    {
        public bool ShouldIgnoreRootReturnUrl =>
            IsMaster || string.Equals(UserType, "Inspector", StringComparison.Ordinal);
    }
}
