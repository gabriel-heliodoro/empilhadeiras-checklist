using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o login do supervisor.")]
    [Display(Name = "Login")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha do supervisor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
