using Checklist.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    private const string UtcNowSqlServerExpression = "SYSUTCDATETIME()";
    private const string LargeTextColumnType = "nvarchar(max)";
    private const string LargeBinaryColumnType = "varbinary(max)";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MvcEquipment> Equipment { get; set; } = null!;
    public DbSet<MvcSubmittedChecklist> SubmittedChecklists { get; set; } = null!;
    public DbSet<MvcOperator> Operators { get; set; } = null!;
    public DbSet<MvcEquipmentCategory> EquipmentCategories { get; set; } = null!;
    public DbSet<MvcChecklistItemTemplate> ChecklistItemTemplates { get; set; } = null!;
    public DbSet<MvcChecklist> Checklists { get; set; } = null!;
    public DbSet<MvcChecklistItem> ChecklistItems { get; set; } = null!;
    public DbSet<MvcChecklistItemAction> ChecklistItemActions { get; set; } = null!;
    public DbSet<MvcChecklistItemActionHistory> ChecklistItemActionHistoryEntries { get; set; } = null!;
    public DbSet<MvcStpAreaChecklistTemplate> StpAreaChecklistTemplates { get; set; } = null!;
    public DbSet<MvcStpAreaChecklistTemplateItem> StpAreaChecklistTemplateItems { get; set; } = null!;
    public DbSet<MvcStpInspectionArea> StpInspectionAreas { get; set; } = null!;
    public DbSet<MvcStpCompanyDocument> StpCompanyDocuments { get; set; } = null!;
    public DbSet<MvcStpCompanyDocumentFile> StpCompanyDocumentFiles { get; set; } = null!;
    public DbSet<MvcStpEmployeeDocument> StpEmployeeDocuments { get; set; } = null!;
    public DbSet<MvcStpEmployeeDocumentFile> StpEmployeeDocumentFiles { get; set; } = null!;
    public DbSet<MvcStpAreaChecklist> StpAreaChecklists { get; set; } = null!;
    public DbSet<MvcStpAreaChecklistItem> StpAreaChecklistItems { get; set; } = null!;
    public DbSet<MvcSector> Sectors { get; set; } = null!;
    public DbSet<MvcSupervisorUser> SupervisorUsers { get; set; } = null!;
    public DbSet<MvcSupervisorUserModule> SupervisorUserModules { get; set; } = null!;
    public DbSet<MvcMonthlyChecklistClosure> MonthlyChecklistClosures { get; set; } = null!;
    public DbSet<MvcMonthlyChecklistClosureChecklist> MonthlyChecklistClosureChecklists { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MvcSector>(entity =>
        {
            entity.ToTable("Setores");
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<MvcEquipmentCategory>(entity =>
        {
            entity.ToTable("CategoriasEquipamento");
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.MonthlyClosureModel).HasConversion<int>();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.Name }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcEquipment>(entity =>
        {
            entity.ToTable("Equipamentos");
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasIndex(x => x.QrId).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category).WithMany(x => x.Equipments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcSubmittedChecklist>(entity =>
        {
            entity.ToTable("ChecklistsEnviados");
            entity.HasOne(x => x.Equipment).WithMany().HasForeignKey(x => x.EquipmentId);
        });

        modelBuilder.Entity<MvcOperator>(entity =>
        {
            entity.ToTable("Operadores");
            entity.Property(x => x.Registration).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Login).HasMaxLength(60).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Extension).HasMaxLength(20);
            entity.Property(x => x.ForceChangePassword).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.Registration }).IsUnique();
            entity.HasIndex(x => x.Login).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcChecklistItemTemplate>(entity =>
        {
            entity.ToTable("ChecklistItensTemplate");
            entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Instruction).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.CategoryId, x.Order }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category).WithMany(x => x.ChecklistItemTemplates).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MvcChecklist>(entity =>
        {
            entity.ToTable("Checklists");
            entity.Property(x => x.GeneralNotes).HasMaxLength(1000);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.EquipmentId, x.ReferenceDate }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Equipment).WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items).WithOne(x => x.Checklist).HasForeignKey(x => x.ChecklistId);
        });

        modelBuilder.Entity<MvcChecklistItem>(entity =>
        {
            entity.ToTable("ChecklistItens");
            entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Instruction).HasMaxLength(500);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.NokImageBase64).HasColumnType(LargeTextColumnType);
            entity.Property(x => x.NokImageFileName).HasMaxLength(260);
            entity.Property(x => x.NokImageMimeType).HasMaxLength(120);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasOne(x => x.Checklist).WithMany(x => x.Items).HasForeignKey(x => x.ChecklistId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Action)
                .WithOne(x => x.ChecklistItem)
                .HasForeignKey<MvcChecklistItemAction>(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MvcChecklistItemAction>(entity =>
        {
            entity.ToTable("ChecklistItensAcoes");
            entity.Property(x => x.AssignmentNotes).HasMaxLength(1000);
            entity.Property(x => x.ResponsibleNotes).HasMaxLength(2000);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.CompletionPercentage).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => x.ChecklistItemId).IsUnique();
            entity.HasOne(x => x.ChecklistItem).WithOne(x => x.Action).HasForeignKey<MvcChecklistItemAction>(x => x.ChecklistItemId);
            entity.HasOne(x => x.ApprovedBySupervisor).WithMany().HasForeignKey(x => x.ApprovedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ResponsibleSupervisor).WithMany().HasForeignKey(x => x.ResponsibleSupervisorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ResponsibleSector).WithMany().HasForeignKey(x => x.ResponsibleSectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CompletedBySupervisor).WithMany().HasForeignKey(x => x.CompletedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.History).WithOne(x => x.ChecklistItemAction).HasForeignKey(x => x.ChecklistItemActionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MvcChecklistItemActionHistory>(entity =>
        {
            entity.ToTable("ChecklistItensAcoesHistorico");
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasOne(x => x.CreatedBySupervisor).WithMany().HasForeignKey(x => x.CreatedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcSupervisorUser>(entity =>
        {
            entity.ToTable("UsuariosSupervisores");
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Login).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Extension).HasMaxLength(20);
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.UserType).HasConversion<int>();
            entity.Property(x => x.ForceChangePassword).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => x.Login).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Modules).WithOne(x => x.SupervisorUser).HasForeignKey(x => x.SupervisorUserId);
        });

        modelBuilder.Entity<MvcSupervisorUserModule>(entity =>
        {
            entity.ToTable("UsuariosSupervisoresModulos");
            entity.Property(x => x.Module).HasConversion<int>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SupervisorUserId, x.Module }).IsUnique();
            entity.HasOne(x => x.SupervisorUser).WithMany(x => x.Modules).HasForeignKey(x => x.SupervisorUserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MvcMonthlyChecklistClosure>(entity =>
        {
            entity.ToTable("FechamentosChecklistMensais");
            entity.Property(x => x.TemplateName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.TemplateVersion).HasMaxLength(40).IsRequired();
            entity.Property(x => x.PdfFileName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PdfSha256Hash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SnapshotJson).HasColumnType(LargeTextColumnType).IsRequired();
            entity.Property(x => x.PdfContent).HasColumnType(LargeBinaryColumnType).IsRequired();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.EquipmentId, x.Year, x.Month }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Equipment).WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedBySupervisor).WithMany().HasForeignKey(x => x.ClosedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Checklists).WithOne(x => x.MonthlyChecklistClosure).HasForeignKey(x => x.MonthlyChecklistClosureId);
        });

        modelBuilder.Entity<MvcMonthlyChecklistClosureChecklist>(entity =>
        {
            entity.ToTable("FechamentosChecklistMensaisChecklists");
            entity.HasIndex(x => new { x.MonthlyChecklistClosureId, x.ChecklistId }).IsUnique();
            entity.HasOne(x => x.MonthlyChecklistClosure).WithMany(x => x.Checklists).HasForeignKey(x => x.MonthlyChecklistClosureId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Checklist).WithMany().HasForeignKey(x => x.ChecklistId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcStpInspectionArea>(entity =>
        {
            entity.ToTable("StpAreasInspecao");
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.Name }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ResponsibleSupervisor).WithMany().HasForeignKey(x => x.ResponsibleSupervisorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcStpAreaChecklistTemplate>(entity =>
        {
            entity.ToTable("StpAreaChecklistTemplates");
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.Code }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items).WithOne(x => x.Template).HasForeignKey(x => x.TemplateId);
        });

        modelBuilder.Entity<MvcStpAreaChecklistTemplateItem>(entity =>
        {
            entity.ToTable("StpAreaChecklistTemplateItens");
            entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Instruction).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.TemplateId, x.Order }).IsUnique();
            entity.HasOne(x => x.Template).WithMany(x => x.Items).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MvcStpAreaChecklist>(entity =>
        {
            entity.ToTable("StpAreaChecklists");
            entity.Property(x => x.PresentResponsibleName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PresentResponsibleRole).HasMaxLength(120);
            entity.Property(x => x.OtherDeviations).HasMaxLength(4000);
            entity.Property(x => x.ObservedPreventiveBehaviors).HasMaxLength(4000);
            entity.Property(x => x.ObservedUnsafeActs).HasMaxLength(4000);
            entity.Property(x => x.VerifiedUnsafeConditions).HasMaxLength(4000);
            entity.Property(x => x.InspectorSignatureBase64).HasColumnType(LargeTextColumnType).IsRequired();
            entity.Property(x => x.PresentResponsibleSignatureBase64).HasColumnType(LargeTextColumnType).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.ReferenceDate });
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InspectedSector).WithMany().HasForeignKey(x => x.InspectedSectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InspectionArea).WithMany().HasForeignKey(x => x.InspectionAreaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InspectorSupervisor).WithMany().HasForeignKey(x => x.InspectorSupervisorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items).WithOne(x => x.Checklist).HasForeignKey(x => x.ChecklistId);
        });

        modelBuilder.Entity<MvcStpAreaChecklistItem>(entity =>
        {
            entity.ToTable("StpAreaChecklistItens");
            entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Instruction).HasMaxLength(2000);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.ChecklistId, x.Order }).IsUnique();
            entity.HasOne(x => x.Checklist).WithMany(x => x.Items).HasForeignKey(x => x.ChecklistId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TemplateItem).WithMany().HasForeignKey(x => x.TemplateItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcStpCompanyDocument>(entity =>
        {
            entity.ToTable("StpDocumentosEmpresas");
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.SectorId, x.Name }).IsUnique();
            entity.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Documents).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId);
            entity.HasMany(x => x.Employees).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId);
        });

        modelBuilder.Entity<MvcStpCompanyDocumentFile>(entity =>
        {
            entity.ToTable("StpDocumentosEmpresasArquivos");
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Content).HasColumnType(LargeBinaryColumnType).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasOne(x => x.Company).WithMany(x => x.Documents).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UploadedBySupervisor).WithMany().HasForeignKey(x => x.UploadedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MvcStpEmployeeDocument>(entity =>
        {
            entity.ToTable("StpDocumentosFuncionarios");
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(160);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.HasOne(x => x.Company).WithMany(x => x.Employees).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Documents).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
        });

        modelBuilder.Entity<MvcStpEmployeeDocumentFile>(entity =>
        {
            entity.ToTable("StpDocumentosFuncionariosArquivos");
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Content).HasColumnType(LargeBinaryColumnType).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql(UtcNowSqlServerExpression);
            entity.HasOne(x => x.Employee).WithMany(x => x.Documents).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UploadedBySupervisor).WithMany().HasForeignKey(x => x.UploadedBySupervisorId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
