using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Checklist.Mvc.ViewModels;

public class StpCompanyListPageViewModel
{
    public StpCompanyFormViewModel Form { get; set; } = new();
    public List<StpCompanyItemViewModel> Items { get; set; } = [];
}

public class StpCompanyItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalEmployees { get; set; }
}

public class StpCompanyFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Nome da empresa")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Empresa ativa")]
    public bool IsActive { get; set; } = true;
}

public class StpCompanyDetailsPageViewModel
{
    public Guid CompanyId { get; set; }
    public StpCompanyFormViewModel CompanyForm { get; set; } = new();
    public StpDocumentUploadFormViewModel UploadForm { get; set; } = new();
    public StpEmployeeFormViewModel EmployeeForm { get; set; } = new();
    public List<StpDocumentItemViewModel> Documents { get; set; } = [];
    public List<StpEmployeeItemViewModel> Employees { get; set; } = [];
}

public class StpEmployeeItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public int TotalDocuments { get; set; }
}

public class StpEmployeeFormViewModel
{
    public Guid? Id { get; set; }
    public Guid CompanyId { get; set; }

    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Cargo")]
    public string? Role { get; set; }

    [Display(Name = "Funcionario ativo")]
    public bool IsActive { get; set; } = true;
}

public class StpEmployeeDetailsPageViewModel
{
    public Guid EmployeeId { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public StpEmployeeFormViewModel EmployeeForm { get; set; } = new();
    public StpDocumentUploadFormViewModel UploadForm { get; set; } = new();
    public List<StpDocumentItemViewModel> Documents { get; set; } = [];
}

public class StpDocumentUploadFormViewModel
{
    [Display(Name = "Nome do documento")]
    public string? Name { get; set; }

    [Display(Name = "Arquivo")]
    public IFormFile? File { get; set; }
}

public class StpDocumentItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
