using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class OperatorLoginViewModel
{
    [Display(Name = "Login")]
    [Required(ErrorMessage = "Informe o login do operador.")]
    public string Login { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe a senha do operador.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
