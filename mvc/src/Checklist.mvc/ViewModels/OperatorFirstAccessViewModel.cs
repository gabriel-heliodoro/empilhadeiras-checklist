using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class OperatorFirstAccessViewModel
{
    public string OperatorName { get; set; } = string.Empty;
    public string OperatorRegistration { get; set; } = string.Empty;

    [Display(Name = "Nova senha")]
    [Required(ErrorMessage = "Informe a nova senha.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirmar senha")]
    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
