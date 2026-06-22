using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Common;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Checklist.Infrastructure.Identity;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SectorSupervisorReady")]
public class CatalogController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly PasswordHashingService _passwordHashingService;

    public CatalogController(
        AppDbContext dbContext,
        ICurrentUser currentUser,
        PasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHashingService = passwordHashingService;
    }

    [HttpGet("catalog/categories")]
    public async Task<IActionResult> Categories([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        return View(await BuildCategoryPageViewModelAsync(sectorId.Value, new CategoryManagementFormViewModel(), editId, cancellationToken));
    }

    [HttpPost("catalog/categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryManagementFormViewModel form, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("Categories", await BuildCategoryPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        var categoryName = (form.Name ?? string.Empty).Trim();
        if (await _dbContext.EquipmentCategories.AnyAsync(
                category => category.SectorId == sectorId.Value
                    && category.Name.ToLower() == categoryName.ToLower()
                    && (!form.Id.HasValue || category.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Name), "Ja existe categoria com este nome.");
            return View("Categories", await BuildCategoryPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        if (!Enum.TryParse<MvcMonthlyClosureModel>(form.MonthlyClosureModel, out var monthlyClosureModel))
        {
            monthlyClosureModel = MvcMonthlyClosureModel.None;
        }

        if (form.Id.HasValue)
        {
            var category = await _dbContext.EquipmentCategories.FirstOrDefaultAsync(
                x => x.Id == form.Id.Value && x.SectorId == sectorId.Value,
                cancellationToken);

            if (category is null)
            {
                return NotFound();
            }

            category.Name = categoryName;
            category.IsActive = form.IsActive;
            category.MonthlyClosureModel = monthlyClosureModel;
        }
        else
        {
            _dbContext.EquipmentCategories.Add(new MvcEquipmentCategory
            {
                SectorId = sectorId.Value,
                Name = categoryName,
                IsActive = form.IsActive,
                MonthlyClosureModel = monthlyClosureModel
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
        {
            ModelState.AddModelError(nameof(form.Name), "Ja existe categoria com este nome.");
            return View("Categories", await BuildCategoryPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        TempData["StatusMessage"] = form.Id.HasValue ? "Categoria atualizada." : "Categoria criada.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Categories), new { editId = form.Id });
    }

    [HttpPost("catalog/categories/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        var category = await _dbContext.EquipmentCategories.FirstOrDefaultAsync(
            x => x.Id == id && x.SectorId == sectorId.Value,
            cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        if (await _dbContext.Equipment.AnyAsync(x => x.CategoryId == id && x.SectorId == sectorId.Value, cancellationToken))
        {
            TempData["StatusMessage"] = "Nao e possivel excluir categoria com equipamentos vinculados.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Categories), new { editId = id });
        }

        _dbContext.EquipmentCategories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Categoria excluida.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("catalog/templates")]
    public async Task<IActionResult> Templates([FromQuery] Guid? categoryId, [FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        return View(await BuildTemplatePageViewModelAsync(sectorId.Value, new TemplateManagementFormViewModel(), categoryId, editId, cancellationToken));
    }

    [HttpPost("catalog/templates")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTemplate(TemplateManagementFormViewModel form, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("Templates", await BuildTemplatePageViewModelAsync(sectorId.Value, form, form.CategoryId, form.Id, cancellationToken));
        }

        var category = await _dbContext.EquipmentCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == form.CategoryId && x.SectorId == sectorId.Value && x.IsActive,
                cancellationToken);

        if (category is null)
        {
            ModelState.AddModelError(nameof(form.CategoryId), "Categoria invalida, inativa ou fora do setor.");
            return View("Templates", await BuildTemplatePageViewModelAsync(sectorId.Value, form, form.CategoryId, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var template = await _dbContext.ChecklistItemTemplates.FirstOrDefaultAsync(
                x => x.Id == form.Id.Value && x.SectorId == sectorId.Value,
                cancellationToken);

            if (template is null)
            {
                return NotFound();
            }

            template.Order = form.Order;
            template.Description = form.Description.Trim();
            template.Instruction = NormalizeOptionalText(form.Instruction);
            template.IsActive = form.IsActive;
        }
        else
        {
            _dbContext.ChecklistItemTemplates.Add(new MvcChecklistItemTemplate
            {
                SectorId = sectorId.Value,
                CategoryId = form.CategoryId,
                Order = form.Order,
                Description = form.Description.Trim(),
                Instruction = NormalizeOptionalText(form.Instruction),
                IsActive = form.IsActive
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
        {
            ModelState.AddModelError(nameof(form.Order), "Ja existe um item com esta ordem para a categoria.");
            return View("Templates", await BuildTemplatePageViewModelAsync(sectorId.Value, form, form.CategoryId, form.Id, cancellationToken));
        }

        TempData["StatusMessage"] = form.Id.HasValue ? "Template atualizado." : "Template criado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Templates), new { categoryId = form.CategoryId });
    }

    [HttpPost("catalog/templates/{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTemplate(Guid id, Guid categoryId, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        var template = await _dbContext.ChecklistItemTemplates.FirstOrDefaultAsync(
            x => x.Id == id && x.SectorId == sectorId.Value,
            cancellationToken);

        if (template is null)
        {
            return NotFound();
        }

        _dbContext.ChecklistItemTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Template excluido.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Templates), new { categoryId });
    }

    [HttpGet("catalog/operators")]
    public async Task<IActionResult> Operators([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        return View(await BuildOperatorPageViewModelAsync(sectorId.Value, new OperatorManagementFormViewModel(), editId, cancellationToken));
    }

    [HttpPost("catalog/operators")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOperator(OperatorManagementFormViewModel form, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("Operators", await BuildOperatorPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        var registration = (form.Registration ?? string.Empty).Trim();
        var normalizedLogin = OperatorLoginNormalizer.Normalize(form.Login);

        if (await _dbContext.Operators.AnyAsync(
                x => x.SectorId == sectorId.Value
                    && x.Registration == registration
                    && (!form.Id.HasValue || x.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Registration), "Ja existe operador com esta matricula neste setor.");
            return View("Operators", await BuildOperatorPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        if (await _dbContext.Operators.AnyAsync(
                x => x.Login == normalizedLogin && (!form.Id.HasValue || x.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Login), "Ja existe operador com este login.");
            return View("Operators", await BuildOperatorPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var op = await _dbContext.Operators.FirstOrDefaultAsync(
                x => x.Id == form.Id.Value && x.SectorId == sectorId.Value,
                cancellationToken);

            if (op is null)
            {
                return NotFound();
            }

            op.Name = form.Name.Trim();
            op.Login = normalizedLogin;
            op.IsActive = form.IsActive;
            op.ForceChangePassword = form.ForceChangePassword;

            if (!string.IsNullOrWhiteSpace(form.Password))
            {
                op.PasswordHash = _passwordHashingService.HashPassword(form.Password);
                op.ForceChangePassword = true;
            }
        }
        else
        {
            _dbContext.Operators.Add(new MvcOperator
            {
                SectorId = sectorId.Value,
                Registration = registration,
                Name = form.Name.Trim(),
                Login = normalizedLogin,
                PasswordHash = _passwordHashingService.HashPassword(form.Password!),
                ForceChangePassword = form.ForceChangePassword,
                IsActive = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = form.Id.HasValue ? "Operador atualizado." : "Operador criado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Operators), new { editId = form.Id });
    }

    [HttpGet("catalog/equipment")]
    public async Task<IActionResult> Equipment([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        return View(await BuildEquipmentPageViewModelAsync(sectorId.Value, new EquipmentManagementFormViewModel(), editId, cancellationToken));
    }

    [HttpPost("catalog/equipment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEquipment(EquipmentManagementFormViewModel form, CancellationToken cancellationToken)
    {
        var sectorId = RequireSectorId();
        if (sectorId is null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View("Equipment", await BuildEquipmentPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        var activeCategory = await _dbContext.EquipmentCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == form.CategoryId && x.SectorId == sectorId.Value && x.IsActive,
                cancellationToken);

        if (activeCategory is null)
        {
            ModelState.AddModelError(nameof(form.CategoryId), "Categoria invalida, inativa ou fora do setor.");
            return View("Equipment", await BuildEquipmentPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        var code = (form.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (!form.Id.HasValue && await _dbContext.Equipment.AnyAsync(
                x => x.SectorId == sectorId.Value && x.Code == code,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Code), "Ja existe equipamento com este codigo neste setor.");
            return View("Equipment", await BuildEquipmentPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var equipment = await _dbContext.Equipment.FirstOrDefaultAsync(
                x => x.Id == form.Id.Value && x.SectorId == sectorId.Value,
                cancellationToken);

            if (equipment is null)
            {
                return NotFound();
            }

            equipment.Description = form.Description.Trim();
            equipment.CategoryId = form.CategoryId;
            equipment.IsActive = form.IsActive;
        }
        else
        {
            _dbContext.Equipment.Add(new MvcEquipment
            {
                SectorId = sectorId.Value,
                Code = code,
                Description = form.Description.Trim(),
                CategoryId = form.CategoryId,
                IsActive = form.IsActive
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
        {
            ModelState.AddModelError(nameof(form.Code), "Ja existe equipamento com este codigo.");
            return View("Equipment", await BuildEquipmentPageViewModelAsync(sectorId.Value, form, form.Id, cancellationToken));
        }

        TempData["StatusMessage"] = form.Id.HasValue ? "Equipamento atualizado." : "Equipamento criado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Equipment), new { editId = form.Id });
    }

    private async Task<CategoryManagementPageViewModel> BuildCategoryPageViewModelAsync(
        Guid sectorId,
        CategoryManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.EquipmentCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.SectorId == sectorId, cancellationToken);

            if (current is not null)
            {
                form = new CategoryManagementFormViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    IsActive = current.IsActive,
                    MonthlyClosureModel = current.MonthlyClosureModel.ToString()
                };
            }
        }

        var items = await _dbContext.EquipmentCategories
            .AsNoTracking()
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryManagementItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                MonthlyClosureModel = x.MonthlyClosureModel.ToString(),
                TemplateCount = _dbContext.ChecklistItemTemplates.Count(template => template.CategoryId == x.Id),
                EquipmentCount = _dbContext.Equipment.Count(equipment => equipment.CategoryId == x.Id)
            })
            .ToListAsync(cancellationToken);

        return new CategoryManagementPageViewModel
        {
            Form = form,
            Items = items
        };
    }

    private async Task<TemplateManagementPageViewModel> BuildTemplatePageViewModelAsync(
        Guid sectorId,
        TemplateManagementFormViewModel form,
        Guid? categoryId,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        var categories = await GetCategoryOptionsAsync(sectorId, cancellationToken);
        Guid? selectedCategoryId = categoryId ?? (form.CategoryId == Guid.Empty ? null : form.CategoryId);
        if (selectedCategoryId == Guid.Empty && categories.Count > 0)
        {
            selectedCategoryId = categories[0].Id;
        }

        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.ChecklistItemTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.SectorId == sectorId, cancellationToken);

            if (current is not null)
            {
                selectedCategoryId = current.CategoryId;
                form = new TemplateManagementFormViewModel
                {
                    Id = current.Id,
                    CategoryId = current.CategoryId,
                    Order = current.Order,
                    Description = current.Description,
                    Instruction = current.Instruction,
                    IsActive = current.IsActive
                };
            }
        }

        if (form.CategoryId == Guid.Empty && selectedCategoryId.HasValue)
        {
            form.CategoryId = selectedCategoryId.Value;
        }

        var items = selectedCategoryId.HasValue
            ? await _dbContext.ChecklistItemTemplates
                .AsNoTracking()
                .Where(x => x.CategoryId == selectedCategoryId.Value && x.SectorId == sectorId)
                .OrderBy(x => x.Order)
                .Select(x => new TemplateManagementItemViewModel
                {
                    Id = x.Id,
                    Order = x.Order,
                    Description = x.Description,
                    Instruction = x.Instruction,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken)
            : [];

        return new TemplateManagementPageViewModel
        {
            Form = form,
            CategoryOptions = categories,
            SelectedCategoryId = selectedCategoryId,
            SelectedCategoryLabel = categories.FirstOrDefault(x => x.Id == selectedCategoryId)?.Label,
            Items = items
        };
    }

    private async Task<OperatorManagementPageViewModel> BuildOperatorPageViewModelAsync(
        Guid sectorId,
        OperatorManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.Operators
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.SectorId == sectorId, cancellationToken);

            if (current is not null)
            {
                form = new OperatorManagementFormViewModel
                {
                    Id = current.Id,
                    Registration = current.Registration,
                    Name = current.Name,
                    Login = current.Login,
                    ForceChangePassword = current.ForceChangePassword,
                    IsActive = current.IsActive
                };
            }
        }

        var items = await _dbContext.Operators
            .AsNoTracking()
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Registration)
            .Select(x => new OperatorManagementItemViewModel
            {
                Id = x.Id,
                Registration = x.Registration,
                Name = x.Name,
                Login = x.Login,
                ForceChangePassword = x.ForceChangePassword,
                IsActive = x.IsActive,
                LastLoginAt = x.LastLoginAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new OperatorManagementPageViewModel
        {
            Form = form,
            Items = items
        };
    }

    private async Task<EquipmentManagementPageViewModel> BuildEquipmentPageViewModelAsync(
        Guid sectorId,
        EquipmentManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.Equipment
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value && x.SectorId == sectorId, cancellationToken);

            if (current is not null)
            {
                form = new EquipmentManagementFormViewModel
                {
                    Id = current.Id,
                    Code = current.Code,
                    Description = current.Description,
                    CategoryId = current.CategoryId,
                    IsActive = current.IsActive
                };
            }
        }

        var categories = await GetCategoryOptionsAsync(sectorId, cancellationToken);
        var items = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Code)
            .Select(x => new EquipmentManagementItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                QrId = x.QrId
            })
            .ToListAsync(cancellationToken);

        return new EquipmentManagementPageViewModel
        {
            Form = form,
            Items = items,
            CategoryOptions = categories
        };
    }

    private async Task<List<ManagementOptionViewModel>> GetCategoryOptionsAsync(Guid sectorId, CancellationToken cancellationToken)
    {
        return await _dbContext.EquipmentCategories
            .AsNoTracking()
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Name)
            .Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToListAsync(cancellationToken);
    }

    private Guid? RequireSectorId()
    {
        return _currentUser.SectorId;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
