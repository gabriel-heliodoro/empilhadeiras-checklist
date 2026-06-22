using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SafetyWorkReady")]
public class StpDocumentsController : Controller
{
    private readonly IStpDocumentControlService _documentControlService;

    public StpDocumentsController(IStpDocumentControlService documentControlService)
    {
        _documentControlService = documentControlService;
    }

    [HttpGet("stp/documents")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await BuildCompanyListPageAsync(new StpCompanyFormViewModel(), cancellationToken));
    }

    [HttpPost("stp/documents/companies")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCompany(StpCompanyFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildCompanyListPageAsync(form, cancellationToken));
        }

        var result = await _documentControlService.SaveCompanyAsync(new StpCompanyUpsertDto
        {
            Id = form.Id,
            Name = form.Name,
            IsActive = form.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel salvar a empresa.");
            return View("Index", await BuildCompanyListPageAsync(form, cancellationToken));
        }

        TempData["StatusMessage"] = form.Id.HasValue ? "Empresa atualizada." : "Empresa criada.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("stp/documents/companies/{companyId:guid}")]
    public async Task<IActionResult> Company(Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetCompanyDetailsAsync(companyId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(MapCompanyPage(result.Value));
    }

    [HttpPost("stp/documents/companies/{companyId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(Guid companyId, StpCompanyDetailsPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        var result = await _documentControlService.SaveCompanyAsync(new StpCompanyUpsertDto
        {
            Id = companyId,
            Name = model.CompanyForm.Name,
            IsActive = model.CompanyForm.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel atualizar a empresa.");
            return View(await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        TempData["StatusMessage"] = "Empresa atualizada.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Company), new { companyId });
    }

    [HttpPost("stp/documents/companies/{companyId:guid}/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCompanyDocument(Guid companyId, StpCompanyDetailsPageViewModel model, CancellationToken cancellationToken)
    {
        if (model.UploadForm.File is null)
        {
            ModelState.AddModelError("UploadForm.File", "Selecione um arquivo.");
            return View("Company", await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        var uploadRequest = await ReadUploadAsync(model.UploadForm.File, model.UploadForm.Name, cancellationToken);
        var result = await _documentControlService.UploadCompanyDocumentAsync(companyId, uploadRequest, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel enviar o documento da empresa.");
            return View("Company", await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        TempData["StatusMessage"] = "Documento da empresa enviado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Company), new { companyId });
    }

    [HttpGet("stp/documents/company-files/{documentId:guid}")]
    public async Task<IActionResult> CompanyFile(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetCompanyDocumentAsync(documentId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound();
        }

        return File(result.Value.Content, result.Value.MimeType, result.Value.FileName, enableRangeProcessing: true);
    }

    [HttpPost("stp/documents/companies/{companyId:guid}/employees")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEmployee(Guid companyId, StpCompanyDetailsPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(model.EmployeeForm, nameof(model.EmployeeForm)))
        {
            return View("Company", await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        var result = await _documentControlService.SaveEmployeeAsync(new StpEmployeeUpsertDto
        {
            Id = model.EmployeeForm.Id,
            CompanyId = companyId,
            Name = model.EmployeeForm.Name,
            Role = model.EmployeeForm.Role,
            IsActive = model.EmployeeForm.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel salvar o funcionario.");
            return View("Company", await MergeCompanyPageAsync(companyId, model, cancellationToken));
        }

        TempData["StatusMessage"] = model.EmployeeForm.Id.HasValue ? "Funcionario atualizado." : "Funcionario criado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Company), new { companyId });
    }

    [HttpGet("stp/documents/employees/{employeeId:guid}")]
    public async Task<IActionResult> Employee(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetEmployeeDetailsAsync(employeeId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(MapEmployeePage(result.Value));
    }

    [HttpPost("stp/documents/employees/{employeeId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Employee(Guid employeeId, StpEmployeeDetailsPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(await MergeEmployeePageAsync(employeeId, model, cancellationToken));
        }

        var result = await _documentControlService.SaveEmployeeAsync(new StpEmployeeUpsertDto
        {
            Id = employeeId,
            CompanyId = model.CompanyId,
            Name = model.EmployeeForm.Name,
            Role = model.EmployeeForm.Role,
            IsActive = model.EmployeeForm.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel atualizar o funcionario.");
            return View(await MergeEmployeePageAsync(employeeId, model, cancellationToken));
        }

        TempData["StatusMessage"] = "Funcionario atualizado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Employee), new { employeeId });
    }

    [HttpPost("stp/documents/employees/{employeeId:guid}/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEmployeeDocument(Guid employeeId, StpEmployeeDetailsPageViewModel model, CancellationToken cancellationToken)
    {
        if (model.UploadForm.File is null)
        {
            ModelState.AddModelError("UploadForm.File", "Selecione um arquivo.");
            return View("Employee", await MergeEmployeePageAsync(employeeId, model, cancellationToken));
        }

        var uploadRequest = await ReadUploadAsync(model.UploadForm.File, model.UploadForm.Name, cancellationToken);
        var result = await _documentControlService.UploadEmployeeDocumentAsync(employeeId, uploadRequest, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel enviar o documento do funcionario.");
            return View("Employee", await MergeEmployeePageAsync(employeeId, model, cancellationToken));
        }

        TempData["StatusMessage"] = "Documento do funcionario enviado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Employee), new { employeeId });
    }

    [HttpGet("stp/documents/employee-files/{documentId:guid}")]
    public async Task<IActionResult> EmployeeFile(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetEmployeeDocumentAsync(documentId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound();
        }

        return File(result.Value.Content, result.Value.MimeType, result.Value.FileName, enableRangeProcessing: true);
    }

    private async Task<StpCompanyListPageViewModel> BuildCompanyListPageAsync(StpCompanyFormViewModel form, CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetCompaniesAsync(cancellationToken);
        return new StpCompanyListPageViewModel
        {
            Form = form,
            Items = result.Value?.Select(x => new StpCompanyItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                TotalDocuments = x.TotalDocuments,
                TotalEmployees = x.TotalEmployees
            }).ToList() ?? []
        };
    }

    private async Task<StpCompanyDetailsPageViewModel> MergeCompanyPageAsync(
        Guid companyId,
        StpCompanyDetailsPageViewModel posted,
        CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetCompanyDetailsAsync(companyId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return posted;
        }

        var model = MapCompanyPage(result.Value);
        model.CompanyForm.Name = posted.CompanyForm.Name;
        model.CompanyForm.IsActive = posted.CompanyForm.IsActive;
        model.UploadForm.Name = posted.UploadForm.Name;
        model.EmployeeForm.Id = posted.EmployeeForm.Id;
        model.EmployeeForm.Name = posted.EmployeeForm.Name;
        model.EmployeeForm.Role = posted.EmployeeForm.Role;
        model.EmployeeForm.IsActive = posted.EmployeeForm.IsActive;
        return model;
    }

    private async Task<StpEmployeeDetailsPageViewModel> MergeEmployeePageAsync(
        Guid employeeId,
        StpEmployeeDetailsPageViewModel posted,
        CancellationToken cancellationToken)
    {
        var result = await _documentControlService.GetEmployeeDetailsAsync(employeeId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return posted;
        }

        var model = MapEmployeePage(result.Value);
        model.EmployeeForm.Name = posted.EmployeeForm.Name;
        model.EmployeeForm.Role = posted.EmployeeForm.Role;
        model.EmployeeForm.IsActive = posted.EmployeeForm.IsActive;
        model.UploadForm.Name = posted.UploadForm.Name;
        return model;
    }

    private static StpCompanyDetailsPageViewModel MapCompanyPage(StpCompanyDetailsDto dto)
    {
        return new StpCompanyDetailsPageViewModel
        {
            CompanyId = dto.Company.Id,
            CompanyForm = new StpCompanyFormViewModel
            {
                Id = dto.Company.Id,
                Name = dto.Company.Name,
                IsActive = dto.Company.IsActive
            },
            EmployeeForm = new StpEmployeeFormViewModel
            {
                CompanyId = dto.Company.Id
            },
            Documents = dto.Documents.Select(MapDocument).ToList(),
            Employees = dto.Employees.Select(x => new StpEmployeeItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Role = x.Role,
                IsActive = x.IsActive,
                TotalDocuments = x.TotalDocuments
            }).ToList()
        };
    }

    private static StpEmployeeDetailsPageViewModel MapEmployeePage(StpEmployeeDetailsDto dto)
    {
        return new StpEmployeeDetailsPageViewModel
        {
            EmployeeId = dto.Employee.Id,
            CompanyId = dto.Company.Id,
            CompanyName = dto.Company.Name,
            EmployeeForm = new StpEmployeeFormViewModel
            {
                Id = dto.Employee.Id,
                CompanyId = dto.Company.Id,
                Name = dto.Employee.Name,
                Role = dto.Employee.Role,
                IsActive = dto.Employee.IsActive
            },
            Documents = dto.Documents.Select(MapDocument).ToList()
        };
    }

    private static StpDocumentItemViewModel MapDocument(StpDocumentFileDto item)
    {
        return new StpDocumentItemViewModel
        {
            Id = item.Id,
            Name = item.Name,
            OriginalFileName = item.OriginalFileName,
            MimeType = item.MimeType,
            SizeInBytes = item.SizeInBytes,
            CreatedAt = item.CreatedAt
        };
    }

    private static async Task<StpDocumentUploadDto> ReadUploadAsync(
        IFormFile file,
        string? documentName,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        return new StpDocumentUploadDto
        {
            Name = documentName,
            OriginalFileName = file.FileName,
            MimeType = file.ContentType,
            SizeInBytes = file.Length,
            Content = stream.ToArray()
        };
    }
}
