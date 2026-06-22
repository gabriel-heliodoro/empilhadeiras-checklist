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

public class AccountController : Controller
{
    private readonly ISupervisorAuthenticationService _authenticationService;

    public AccountController(ISupervisorAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
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

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(MvcAuthenticationSchemes.Supervisor);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
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
}
