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

[Route("operador")]
public class OperatorAccountController : Controller
{
    private readonly IOperatorAuthenticationService _authenticationService;
    private readonly ICurrentOperator _currentOperator;

    public OperatorAccountController(
        IOperatorAuthenticationService authenticationService,
        ICurrentOperator currentOperator)
    {
        _authenticationService = authenticationService;
        _currentOperator = currentOperator;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_currentOperator.IsAuthenticated)
        {
            return RedirectAfterLogin(returnUrl);
        }

        return View(new OperatorLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(OperatorLoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.AuthenticateAsync(model.Login, model.Password, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel autenticar o operador.");
            return View(model);
        }

        await SignInOperatorAsync(result.Value);
        return RedirectAfterLogin(model.ReturnUrl);
    }

    [Authorize(Policy = "OperatorAuthenticated")]
    [HttpGet("primeiro-acesso")]
    public IActionResult FirstAccess()
    {
        if (!_currentOperator.ForceChangePassword)
        {
            return RedirectToAction("Index", "Operator");
        }

        return View(new OperatorFirstAccessViewModel
        {
            OperatorName = _currentOperator.Name ?? string.Empty,
            OperatorRegistration = _currentOperator.Registration ?? string.Empty
        });
    }

    [Authorize(Policy = "OperatorAuthenticated")]
    [HttpPost("primeiro-acesso")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FirstAccess(OperatorFirstAccessViewModel model, CancellationToken cancellationToken)
    {
        model.OperatorName = _currentOperator.Name ?? string.Empty;
        model.OperatorRegistration = _currentOperator.Registration ?? string.Empty;

        if (!_currentOperator.Id.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.ChangePasswordAsync(
            _currentOperator.Id.Value,
            model.NewPassword,
            model.ConfirmPassword,
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel atualizar a senha do operador.");
            return View(model);
        }

        await SignInOperatorAsync(result.Value);
        TempData["OperatorSuccessMessage"] = "Senha atualizada. O checklist operacional ja pode ser iniciado.";
        return RedirectToAction("Index", "Operator");
    }

    [Authorize(Policy = "OperatorAuthenticated")]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(MvcAuthenticationSchemes.Operator);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (_currentOperator.ForceChangePassword)
        {
            return RedirectToAction(nameof(FirstAccess));
        }

        return RedirectToAction("Index", "Operator");
    }

    private async Task SignInOperatorAsync(OperatorSessionDto session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.Id.ToString()),
            new(ClaimTypes.Name, session.Name),
            new(CurrentOperatorClaimTypes.OperatorId, session.Id.ToString()),
            new(CurrentOperatorClaimTypes.SectorId, session.SectorId.ToString()),
            new(CurrentOperatorClaimTypes.SectorName, session.SectorName),
            new(CurrentOperatorClaimTypes.ForceChangePassword, session.ForceChangePassword.ToString().ToLowerInvariant()),
            new(CurrentOperatorClaimTypes.Registration, session.Registration)
        };

        var identity = new ClaimsIdentity(claims, MvcAuthenticationSchemes.Operator);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            MvcAuthenticationSchemes.Operator,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });
    }
}
