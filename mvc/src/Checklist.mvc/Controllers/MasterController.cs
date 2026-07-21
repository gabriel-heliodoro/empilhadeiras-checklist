using Checklist.Application.Common;
using Checklist.Infrastructure.Common;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Checklist.Infrastructure.Identity;
using Checklist.Infrastructure.Services;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "MasterReady")]
public class MasterController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly SupervisorLoginGenerator _supervisorLoginGenerator;
    private readonly ChecklistStandardCatalogService _checklistStandardCatalogService;
    private readonly StpAreaTemplateCatalogService _stpAreaTemplateCatalogService;

    public MasterController(
        AppDbContext dbContext,
        SupervisorLoginGenerator supervisorLoginGenerator,
        ChecklistStandardCatalogService checklistStandardCatalogService,
        StpAreaTemplateCatalogService stpAreaTemplateCatalogService)
    {
        _dbContext = dbContext;
        _supervisorLoginGenerator = supervisorLoginGenerator;
        _checklistStandardCatalogService = checklistStandardCatalogService;
        _stpAreaTemplateCatalogService = stpAreaTemplateCatalogService;
    }

    [HttpGet("master/sectors")]
    public async Task<IActionResult> Sectors([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var model = await BuildSectorPageViewModelAsync(new SectorManagementFormViewModel(), editId, cancellationToken);
        return View(model);
    }

    [HttpPost("master/sectors")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSector(SectorManagementFormViewModel form, CancellationToken cancellationToken)
    {
        var normalizedName = (form.Name ?? string.Empty).Trim();
        var normalizedDescription = NormalizeOptionalText(form.Description);

        if (!ModelState.IsValid)
        {
            return View("Sectors", await BuildSectorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        if (await _dbContext.Sectors.AnyAsync(
                sector => sector.Name.ToLower() == normalizedName.ToLower() && (!form.Id.HasValue || sector.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Name), "Ja existe um setor com este nome.");
            return View("Sectors", await BuildSectorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var sector = await _dbContext.Sectors.FirstOrDefaultAsync(x => x.Id == form.Id.Value, cancellationToken);
            if (sector is null)
            {
                return NotFound();
            }

            sector.Name = normalizedName;
            sector.Description = normalizedDescription;
            sector.IsActive = form.IsActive;
        }
        else
        {
            var sector = new MvcSector
            {
                Name = normalizedName,
                Description = normalizedDescription,
                IsActive = form.IsActive
            };

            _dbContext.Sectors.Add(sector);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _checklistStandardCatalogService.EnsureDefaultsForSectorAsync(sector.Id, cancellationToken);
            await _stpAreaTemplateCatalogService.EnsureDefaultsForSectorAsync(sector.Id, cancellationToken);

            TempData["StatusMessage"] = "Setor criado com os catalogos padrao.";
            TempData["StatusType"] = "success";
            return RedirectToAction(nameof(Sectors), new { editId = sector.Id });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Setor atualizado.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Sectors), new { editId = form.Id });
    }

    [HttpGet("master/supervisors")]
    public async Task<IActionResult> Supervisors([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var model = await BuildSupervisorPageViewModelAsync(isInspector: false, new SupervisorManagementFormViewModel(), editId, cancellationToken);
        return View(model);
    }

    [HttpPost("master/supervisors")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSupervisor(SupervisorManagementFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Supervisors", await BuildSupervisorPageViewModelAsync(false, form, form.Id, cancellationToken));
        }

        var validationError = await ValidateSupervisorCommonDataAsync(form, form.Id, cancellationToken);
        if (validationError is not null)
        {
            ModelState.AddModelError(validationError.Value.Key, validationError.Value.Value);
            return View("Supervisors", await BuildSupervisorPageViewModelAsync(false, form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var user = await _dbContext.SupervisorUsers
                .FirstOrDefaultAsync(
                    x => x.Id == form.Id.Value && !x.IsMaster && x.UserType == MvcUserAccessType.Supervisor,
                    cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            await ApplySupervisorFormAsync(user, form, isInspector: false, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["StatusMessage"] = "Supervisor atualizado.";
        }
        else
        {
            var user = new MvcSupervisorUser
            {
                Name = form.Name.Trim(),
                LastName = form.LastName.Trim(),
                Login = await _supervisorLoginGenerator.GenerateUniqueLoginAsync(form.Name.Trim(), form.LastName.Trim(), null, cancellationToken),
                Email = NormalizeOptionalEmail(form.Email),
                Extension = NormalizeOptionalText(form.Extension),
                IsMaster = false,
                UserType = MvcUserAccessType.Supervisor,
                SectorId = form.SectorId,
                IsActive = form.IsActive
            };

            user.Modules = BuildModules(user.Id, isInspector: false, form);
            _dbContext.SupervisorUsers.Add(user);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
            {
                ModelState.AddModelError(nameof(form.Email), "Ja existe um usuario com este email.");
                return View("Supervisors", await BuildSupervisorPageViewModelAsync(false, form, form.Id, cancellationToken));
            }

            TempData["StatusMessage"] = "Supervisor criado.";
            form.Id = user.Id;
        }

        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Supervisors), new { editId = form.Id });
    }

    [HttpGet("master/inspectors")]
    public async Task<IActionResult> Inspectors([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var model = await BuildSupervisorPageViewModelAsync(isInspector: true, new SupervisorManagementFormViewModel(), editId, cancellationToken);
        return View(model);
    }

    [HttpPost("master/inspectors")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveInspector(SupervisorManagementFormViewModel form, CancellationToken cancellationToken)
    {
        if (!form.WorkSafetyModule && !form.MaterialInspectionModule)
        {
            ModelState.AddModelError(nameof(form.WorkSafetyModule), "Selecione pelo menos um modulo.");
        }

        if (!ModelState.IsValid)
        {
            return View("Inspectors", await BuildSupervisorPageViewModelAsync(true, form, form.Id, cancellationToken));
        }

        var validationError = await ValidateSupervisorCommonDataAsync(form, form.Id, cancellationToken);
        if (validationError is not null)
        {
            ModelState.AddModelError(validationError.Value.Key, validationError.Value.Value);
            return View("Inspectors", await BuildSupervisorPageViewModelAsync(true, form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var user = await _dbContext.SupervisorUsers
                .FirstOrDefaultAsync(
                    x => x.Id == form.Id.Value && !x.IsMaster && x.UserType == MvcUserAccessType.Inspector,
                    cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            await ApplySupervisorFormAsync(user, form, isInspector: true, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            TempData["StatusMessage"] = "Inspetor atualizado.";
        }
        else
        {
            var user = new MvcSupervisorUser
            {
                Name = form.Name.Trim(),
                LastName = form.LastName.Trim(),
                Login = await _supervisorLoginGenerator.GenerateUniqueLoginAsync(form.Name.Trim(), form.LastName.Trim(), null, cancellationToken),
                Email = NormalizeOptionalEmail(form.Email),
                Extension = NormalizeOptionalText(form.Extension),
                IsMaster = false,
                UserType = MvcUserAccessType.Inspector,
                SectorId = form.SectorId,
                IsActive = form.IsActive
            };

            user.Modules = BuildModules(user.Id, isInspector: true, form);
            _dbContext.SupervisorUsers.Add(user);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
            {
                ModelState.AddModelError(nameof(form.Email), "Ja existe um usuario com este email.");
                return View("Inspectors", await BuildSupervisorPageViewModelAsync(true, form, form.Id, cancellationToken));
            }

            TempData["StatusMessage"] = "Inspetor criado.";
            form.Id = user.Id;
        }

        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Inspectors), new { editId = form.Id });
    }

    [HttpGet("master/operators")]
    public async Task<IActionResult> Operators([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        var model = await BuildMasterOperatorPageViewModelAsync(new MasterOperatorManagementFormViewModel(), editId, cancellationToken);
        return View(model);
    }

    [HttpPost("master/operators")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOperator(MasterOperatorManagementFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Operators", await BuildMasterOperatorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        if (!await _dbContext.Sectors.AnyAsync(x => x.Id == form.SectorId && x.IsActive, cancellationToken))
        {
            ModelState.AddModelError(nameof(form.SectorId), "Setor invalido ou inativo.");
            return View("Operators", await BuildMasterOperatorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        var registration = (form.Registration ?? string.Empty).Trim();
        var normalizedLogin = OperatorLoginNormalizer.Normalize(form.Login);

        if (await _dbContext.Operators.AnyAsync(
                x => x.SectorId == form.SectorId
                    && x.Registration == registration
                    && (!form.Id.HasValue || x.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Registration), "Ja existe operador com esta matricula neste setor.");
            return View("Operators", await BuildMasterOperatorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        if (await _dbContext.Operators.AnyAsync(
                x => x.Login == normalizedLogin && (!form.Id.HasValue || x.Id != form.Id.Value),
                cancellationToken))
        {
            ModelState.AddModelError(nameof(form.Login), "Ja existe operador com este login.");
            return View("Operators", await BuildMasterOperatorPageViewModelAsync(form, form.Id, cancellationToken));
        }

        if (form.Id.HasValue)
        {
            var op = await _dbContext.Operators.FirstOrDefaultAsync(x => x.Id == form.Id.Value, cancellationToken);
            if (op is null)
            {
                return NotFound();
            }

            op.Name = form.Name.Trim();
            op.LastName = form.LastName.Trim();
            op.Email = NormalizeOptionalEmail(form.Email);
            op.Extension = NormalizeOptionalText(form.Extension);
            op.Login = normalizedLogin;
            op.SectorId = form.SectorId;
            op.IsActive = form.IsActive;

            TempData["StatusMessage"] = "Operador atualizado.";
        }
        else
        {
            var op = new MvcOperator
            {
                SectorId = form.SectorId,
                Registration = registration,
                Name = form.Name.Trim(),
                LastName = form.LastName.Trim(),
                Email = NormalizeOptionalEmail(form.Email),
                Extension = NormalizeOptionalText(form.Extension),
                Login = normalizedLogin,
                IsActive = form.IsActive
            };

            _dbContext.Operators.Add(op);
            TempData["StatusMessage"] = "Operador criado.";
            form.Id = op.Id;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Operators), new { editId = form.Id });
    }

    private async Task<SectorManagementPageViewModel> BuildSectorPageViewModelAsync(
        SectorManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.Sectors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value, cancellationToken);

            if (current is not null)
            {
                form = new SectorManagementFormViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    Description = current.Description,
                    IsActive = current.IsActive
                };
            }
        }

        var items = await _dbContext.Sectors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SectorManagementItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                SupervisorCount = _dbContext.SupervisorUsers.Count(user => !user.IsMaster && user.SectorId == x.Id),
                EquipmentCount = _dbContext.Equipment.Count(equipment => equipment.SectorId == x.Id),
                OperatorCount = _dbContext.Operators.Count(op => op.SectorId == x.Id)
            })
            .ToListAsync(cancellationToken);

        return new SectorManagementPageViewModel
        {
            Form = form,
            Items = items
        };
    }

    private async Task<MasterOperatorManagementPageViewModel> BuildMasterOperatorPageViewModelAsync(
        MasterOperatorManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.Operators
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == editId.Value, cancellationToken);

            if (current is not null)
            {
                form = new MasterOperatorManagementFormViewModel
                {
                    Id = current.Id,
                    Registration = current.Registration,
                    Name = current.Name,
                    LastName = current.LastName,
                    Login = current.Login,
                    SectorId = current.SectorId,
                    Email = current.Email,
                    Extension = current.Extension,
                    IsActive = current.IsActive
                };
            }
        }

        var items = await _dbContext.Operators
            .AsNoTracking()
            .Include(x => x.Sector)
            .OrderBy(x => x.Sector.Name)
            .ThenBy(x => x.Registration)
            .Select(x => new MasterOperatorManagementItemViewModel
            {
                Id = x.Id,
                Registration = x.Registration,
                Name = x.Name,
                LastName = x.LastName,
                Login = x.Login,
                Email = x.Email,
                Extension = x.Extension,
                IsActive = x.IsActive,
                SectorId = x.SectorId,
                SectorName = x.Sector.Name,
                LastLoginAt = x.LastLoginAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var sectors = await GetSectorOptionsAsync(cancellationToken);

        return new MasterOperatorManagementPageViewModel
        {
            Form = form,
            Items = items,
            SectorOptions = sectors
        };
    }

    private async Task<SupervisorManagementPageViewModel> BuildSupervisorPageViewModelAsync(
        bool isInspector,
        SupervisorManagementFormViewModel form,
        Guid? editId,
        CancellationToken cancellationToken)
    {
        if (editId.HasValue && form.Id is null)
        {
            var current = await _dbContext.SupervisorUsers
                .AsNoTracking()
                .Include(x => x.Modules)
                .FirstOrDefaultAsync(
                    x => x.Id == editId.Value && !x.IsMaster && x.UserType == (isInspector ? MvcUserAccessType.Inspector : MvcUserAccessType.Supervisor),
                    cancellationToken);

            if (current is not null)
            {
                form = new SupervisorManagementFormViewModel
                {
                    Id = current.Id,
                    Name = current.Name,
                    LastName = current.LastName,
                    SectorId = current.SectorId,
                    Email = current.Email,
                    Extension = current.Extension,
                    IsActive = current.IsActive,
                    WorkSafetyModule = current.Modules.Any(x => x.Module == MvcAccessModule.WorkSafety),
                    MaterialInspectionModule = current.Modules.Any(x => x.Module == MvcAccessModule.MaterialInspection)
                };
            }
        }

        var users = await _dbContext.SupervisorUsers
            .AsNoTracking()
            .Include(x => x.Sector)
            .Include(x => x.Modules)
            .Where(x => !x.IsMaster && x.UserType == (isInspector ? MvcUserAccessType.Inspector : MvcUserAccessType.Supervisor))
            .OrderBy(x => x.Sector.Name)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        var sectors = await GetSectorOptionsAsync(cancellationToken);

        return new SupervisorManagementPageViewModel
        {
            Title = isInspector ? "Inspetores" : "Supervisores",
            Subtitle = isInspector
                ? "Gestao dos usuarios de seguranca do trabalho e inspecao de materiais."
                : "Gestao dos supervisores operacionais por setor.",
            IsInspector = isInspector,
            Form = form,
            SectorOptions = sectors,
            Items = users.Select(user => new SupervisorManagementItemViewModel
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Login = user.Login,
                Email = user.Email,
                Extension = user.Extension,
                IsActive = user.IsActive,
                SectorId = user.SectorId,
                SectorName = user.Sector.Name,
                UserType = user.UserType.ToString(),
                ModuleCodes = user.Modules.Select(x => MapModuleCode(x.Module)).ToList()
            }).ToList()
        };
    }

    private async Task<KeyValuePair<string, string>?> ValidateSupervisorCommonDataAsync(
        SupervisorManagementFormViewModel form,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Sectors.AnyAsync(x => x.Id == form.SectorId && x.IsActive, cancellationToken))
        {
            return new KeyValuePair<string, string>(nameof(form.SectorId), "Setor invalido ou inativo.");
        }

        var normalizedEmail = NormalizeOptionalEmail(form.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail)
            && await _dbContext.SupervisorUsers.AnyAsync(
                x => x.Email == normalizedEmail && (!userId.HasValue || x.Id != userId.Value),
                cancellationToken))
        {
            return new KeyValuePair<string, string>(nameof(form.Email), "Ja existe um usuario com este email.");
        }

        return null;
    }

    private async Task ApplySupervisorFormAsync(
        MvcSupervisorUser user,
        SupervisorManagementFormViewModel form,
        bool isInspector,
        CancellationToken cancellationToken)
    {
        user.Name = form.Name.Trim();
        user.LastName = form.LastName.Trim();
        user.Login = await _supervisorLoginGenerator.GenerateUniqueLoginAsync(user.Name, user.LastName, user.Id, cancellationToken);
        user.Email = NormalizeOptionalEmail(form.Email);
        user.Extension = NormalizeOptionalText(form.Extension);
        user.SectorId = form.SectorId;
        user.IsActive = form.IsActive;
        user.UserType = isInspector ? MvcUserAccessType.Inspector : MvcUserAccessType.Supervisor;

        await _dbContext.SupervisorUserModules
            .Where(x => x.SupervisorUserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);
        _dbContext.SupervisorUserModules.AddRange(BuildModules(user.Id, isInspector, form));
    }

    private static List<MvcSupervisorUserModule> BuildModules(Guid userId, bool isInspector, SupervisorManagementFormViewModel form)
    {
        var modules = new List<MvcAccessModule>();
        if (isInspector)
        {
            if (form.WorkSafetyModule)
            {
                modules.Add(MvcAccessModule.WorkSafety);
            }

            if (form.MaterialInspectionModule)
            {
                modules.Add(MvcAccessModule.MaterialInspection);
            }
        }
        else
        {
            modules.Add(MvcAccessModule.OperationalSupervision);
        }

        return modules
            .Distinct()
            .Select(module => new MvcSupervisorUserModule
            {
                SupervisorUserId = userId,
                Module = module
            })
            .ToList();
    }

    private async Task<List<ManagementOptionViewModel>> GetSectorOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Sectors
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeOptionalEmail(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }

    private static string MapModuleCode(MvcAccessModule module)
    {
        return module switch
        {
            MvcAccessModule.OperationalSupervision => AccessModuleCodes.OperationalSupervision,
            MvcAccessModule.WorkSafety => AccessModuleCodes.WorkSafety,
            MvcAccessModule.MaterialInspection => AccessModuleCodes.MaterialInspection,
            _ => module.ToString()
        };
    }
}
