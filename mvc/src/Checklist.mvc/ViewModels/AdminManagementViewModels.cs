using System.ComponentModel.DataAnnotations;
using Checklist.Application.Common;

namespace Checklist.Mvc.ViewModels;

public class ManagementOptionViewModel
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class SectorManagementPageViewModel
{
    public List<SectorManagementItemViewModel> Items { get; set; } = [];
    public SectorManagementFormViewModel Form { get; set; } = new();
}

public class SectorManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SupervisorCount { get; set; }
    public int EquipmentCount { get; set; }
    public int OperatorCount { get; set; }
}

public class SectorManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do setor.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descricao")]
    public string? Description { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;
}

public class SupervisorManagementPageViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool IsInspector { get; set; }
    public List<SupervisorManagementItemViewModel> Items { get; set; } = [];
    public SupervisorManagementFormViewModel Form { get; set; } = new();
    public List<ManagementOptionViewModel> SectorOptions { get; set; } = [];
}

public class SupervisorManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{Name} {LastName}".Trim();
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Extension { get; set; }
    public bool IsActive { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public IReadOnlyList<string> ModuleCodes { get; set; } = [];
}

public class SupervisorManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o sobrenome.")]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o setor.")]
    [Display(Name = "Setor")]
    public Guid SectorId { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Informe um email valido.")]
    public string? Email { get; set; }

    [Display(Name = "Ramal")]
    public string? Extension { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Seguranca do trabalho")]
    public bool WorkSafetyModule { get; set; }

    [Display(Name = "Inspecao de materiais")]
    public bool MaterialInspectionModule { get; set; }
}

public class CategoryManagementPageViewModel
{
    public List<CategoryManagementItemViewModel> Items { get; set; } = [];
    public CategoryManagementFormViewModel Form { get; set; } = new();
}

public class CategoryManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string MonthlyClosureModel { get; set; } = string.Empty;
    public int TemplateCount { get; set; }
    public int EquipmentCount { get; set; }
}

public class CategoryManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Modelo de fechamento mensal")]
    public string MonthlyClosureModel { get; set; } = "None";
}

public class TemplateManagementPageViewModel
{
    public List<TemplateManagementItemViewModel> Items { get; set; } = [];
    public TemplateManagementFormViewModel Form { get; set; } = new();
    public List<ManagementOptionViewModel> CategoryOptions { get; set; } = [];
    public Guid? SelectedCategoryId { get; set; }
    public string? SelectedCategoryLabel { get; set; }
}

public class TemplateManagementItemViewModel
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public bool IsActive { get; set; }
}

public class TemplateManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Selecione a categoria.")]
    [Display(Name = "Categoria")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Informe a ordem.")]
    [Range(1, 999, ErrorMessage = "Informe uma ordem valida.")]
    [Display(Name = "Ordem")]
    public int Order { get; set; }

    [Required(ErrorMessage = "Informe a descricao.")]
    [Display(Name = "Descricao")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Instrucao")]
    public string? Instruction { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;
}

public class OperatorManagementPageViewModel
{
    public List<OperatorManagementItemViewModel> Items { get; set; } = [];
    public OperatorManagementFormViewModel Form { get; set; } = new();
}

public class OperatorManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{Name} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Extension { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OperatorManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe a matricula.")]
    [Display(Name = "Matricula")]
    public string Registration { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o login.")]
    [Display(Name = "Login")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o sobrenome.")]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Informe um email valido.")]
    public string? Email { get; set; }

    [Display(Name = "Ramal")]
    public string? Extension { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;
}

public class MasterOperatorManagementPageViewModel
{
    public List<MasterOperatorManagementItemViewModel> Items { get; set; } = [];
    public MasterOperatorManagementFormViewModel Form { get; set; } = new();
    public List<ManagementOptionViewModel> SectorOptions { get; set; } = [];
}

public class MasterOperatorManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{Name} {LastName}".Trim();
    public string Login { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Extension { get; set; }
    public bool IsActive { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MasterOperatorManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe a matricula.")]
    [Display(Name = "Matricula")]
    public string Registration { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o sobrenome.")]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o login.")]
    [Display(Name = "Login")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o setor.")]
    [Display(Name = "Setor")]
    public Guid SectorId { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Informe um email valido.")]
    public string? Email { get; set; }

    [Display(Name = "Ramal")]
    public string? Extension { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;
}

public class EquipmentManagementPageViewModel
{
    public List<EquipmentManagementItemViewModel> Items { get; set; } = [];
    public EquipmentManagementFormViewModel Form { get; set; } = new();
    public List<ManagementOptionViewModel> CategoryOptions { get; set; } = [];
}

public class EquipmentManagementItemViewModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid QrId { get; set; }
}

public class EquipmentManagementFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o codigo.")]
    [Display(Name = "Codigo")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descricao.")]
    [Display(Name = "Descricao")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione a categoria.")]
    [Display(Name = "Categoria")]
    public Guid CategoryId { get; set; }

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;
}

public static class AdminManagementFormatting
{
    public static string GetModuleLabel(string code)
    {
        return code switch
        {
            AccessModuleCodes.OperationalSupervision => "Supervisao operacional",
            AccessModuleCodes.WorkSafety => "Seguranca do trabalho",
            AccessModuleCodes.MaterialInspection => "Inspecao de materiais",
            _ => code
        };
    }
}
