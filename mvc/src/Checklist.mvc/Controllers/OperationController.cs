using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[AllowAnonymous]
[Route("operacao")]
public class OperationController : Controller
{
    private readonly ICurrentOperator _currentOperator;

    public OperationController(ICurrentOperator currentOperator)
    {
        _currentOperator = currentOperator;
    }

    [HttpGet("")]
    public IActionResult Index([FromQuery] string? sucesso = null)
    {
        if (string.Equals(sucesso, "1", StringComparison.Ordinal))
        {
            TempData["OperationSuccessMessage"] = "Checklist enviado com sucesso. Voce ja pode iniciar uma nova inspecao.";
        }

        return View(new OperationStartViewModel
        {
            IsOperatorAuthenticated = _currentOperator.IsAuthenticated,
            OperatorName = _currentOperator.Name
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(OperationStartViewModel model)
    {
        model = new OperationStartViewModel
        {
            QrId = model.QrId,
            IsOperatorAuthenticated = _currentOperator.IsAuthenticated,
            OperatorName = _currentOperator.Name
        };

        var normalizedQrId = (model.QrId ?? string.Empty).Trim();
        if (!Guid.TryParse(normalizedQrId, out var qrId))
        {
            ModelState.AddModelError(nameof(OperationStartViewModel.QrId), "Digite um QR ID valido.");
            return View(model);
        }

        var returnUrl = Url.Action("ChecklistByQr", "Operator", new { qrId }) ?? $"/operador/checklists/qr/{qrId}";

        if (!_currentOperator.IsAuthenticated)
        {
            return RedirectToAction("Login", "OperatorAccount", new { returnUrl });
        }

        return Redirect(returnUrl);
    }
}
