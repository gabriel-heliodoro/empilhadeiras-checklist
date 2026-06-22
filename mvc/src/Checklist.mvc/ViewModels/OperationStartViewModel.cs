using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class OperationStartViewModel
{
    [Display(Name = "QR ID")]
    [Required(ErrorMessage = "Digite um QR ID valido.")]
    public string QrId { get; set; } = string.Empty;

    public bool IsOperatorAuthenticated { get; init; }
    public string? OperatorName { get; init; }
}
