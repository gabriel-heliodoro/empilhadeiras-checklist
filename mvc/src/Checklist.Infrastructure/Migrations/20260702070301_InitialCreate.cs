using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Checklist.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Setores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasEquipamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MonthlyClosureModel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasEquipamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasEquipamento_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operadores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Registration = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ForceChangePassword = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operadores_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpAreaChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpAreaChecklistTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklistTemplates_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpDocumentosEmpresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpDocumentosEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpDocumentosEmpresas_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSupervisores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ForceChangePassword = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsMaster = table.Column<bool>(type: "bit", nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSupervisores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSupervisores_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItensTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItensTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItensTemplate_CategoriasEquipamento_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CategoriasEquipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistItensTemplate_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipamentos_CategoriasEquipamento_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CategoriasEquipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipamentos_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpAreaChecklistTemplateItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpAreaChecklistTemplateItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklistTemplateItens_StpAreaChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "StpAreaChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StpDocumentosFuncionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpDocumentosFuncionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpDocumentosFuncionarios_StpDocumentosEmpresas_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "StpDocumentosEmpresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StpAreasInspecao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ResponsibleSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpAreasInspecao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpAreasInspecao_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreasInspecao_UsuariosSupervisores_ResponsibleSupervisorId",
                        column: x => x.ResponsibleSupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpDocumentosEmpresasArquivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpDocumentosEmpresasArquivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpDocumentosEmpresasArquivos_StpDocumentosEmpresas_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "StpDocumentosEmpresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StpDocumentosEmpresasArquivos_UsuariosSupervisores_UploadedBySupervisorId",
                        column: x => x.UploadedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSupervisoresModulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupervisorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSupervisoresModulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSupervisoresModulos_UsuariosSupervisores_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Checklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    GeneralNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OperatorSignatureBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checklists_Equipamentos_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checklists_Operadores_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checklists_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistsEnviados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperatorRegistration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistsEnviados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistsEnviados_Equipamentos_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FechamentosChecklistMensais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClosedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TemplateVersion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfFileName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    PdfSha256Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PdfContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ChecklistCount = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechamentosChecklistMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FechamentosChecklistMensais_Equipamentos_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FechamentosChecklistMensais_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FechamentosChecklistMensais_UsuariosSupervisores_ClosedBySupervisorId",
                        column: x => x.ClosedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpDocumentosFuncionariosArquivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpDocumentosFuncionariosArquivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpDocumentosFuncionariosArquivos_StpDocumentosFuncionarios_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "StpDocumentosFuncionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StpDocumentosFuncionariosArquivos_UsuariosSupervisores_UploadedBySupervisorId",
                        column: x => x.UploadedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StpAreaChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectedSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectorSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PresentResponsibleName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    PresentResponsibleRole = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    OtherDeviations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ObservedPreventiveBehaviors = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ObservedUnsafeActs = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    VerifiedUnsafeConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InspectorSignatureBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PresentResponsibleSignatureBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectorSignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PresentResponsibleSignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpAreaChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklists_Setores_InspectedSectorId",
                        column: x => x.InspectedSectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklists_Setores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklists_StpAreaChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "StpAreaChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklists_StpAreasInspecao_InspectionAreaId",
                        column: x => x.InspectionAreaId,
                        principalTable: "StpAreasInspecao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklists_UsuariosSupervisores_InspectorSupervisorId",
                        column: x => x.InspectorSupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NokImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NokImageFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    NokImageMimeType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItens_ChecklistItensTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChecklistItensTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChecklistItens_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FechamentosChecklistMensaisChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthlyChecklistClosureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechamentosChecklistMensaisChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FechamentosChecklistMensaisChecklists_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FechamentosChecklistMensaisChecklists_FechamentosChecklistMensais_MonthlyChecklistClosureId",
                        column: x => x.MonthlyChecklistClosureId,
                        principalTable: "FechamentosChecklistMensais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StpAreaChecklistItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StpAreaChecklistItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklistItens_StpAreaChecklistTemplateItens_TemplateItemId",
                        column: x => x.TemplateItemId,
                        principalTable: "StpAreaChecklistTemplateItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StpAreaChecklistItens_StpAreaChecklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalTable: "StpAreaChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItensAcoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponsibleSupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsibleSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignmentNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResponsibleNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PlannedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItensAcoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoes_ChecklistItens_ChecklistItemId",
                        column: x => x.ChecklistItemId,
                        principalTable: "ChecklistItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoes_Setores_ResponsibleSectorId",
                        column: x => x.ResponsibleSectorId,
                        principalTable: "Setores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoes_UsuariosSupervisores_ApprovedBySupervisorId",
                        column: x => x.ApprovedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoes_UsuariosSupervisores_CompletedBySupervisorId",
                        column: x => x.CompletedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoes_UsuariosSupervisores_ResponsibleSupervisorId",
                        column: x => x.ResponsibleSupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItensAcoesHistorico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistItemActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBySupervisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItensAcoesHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoesHistorico_ChecklistItensAcoes_ChecklistItemActionId",
                        column: x => x.ChecklistItemActionId,
                        principalTable: "ChecklistItensAcoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistItensAcoesHistorico_UsuariosSupervisores_CreatedBySupervisorId",
                        column: x => x.CreatedBySupervisorId,
                        principalTable: "UsuariosSupervisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasEquipamento_SectorId_Name",
                table: "CategoriasEquipamento",
                columns: new[] { "SectorId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItens_ChecklistId",
                table: "ChecklistItens",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItens_TemplateId",
                table: "ChecklistItens",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoes_ApprovedBySupervisorId",
                table: "ChecklistItensAcoes",
                column: "ApprovedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoes_ChecklistItemId",
                table: "ChecklistItensAcoes",
                column: "ChecklistItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoes_CompletedBySupervisorId",
                table: "ChecklistItensAcoes",
                column: "CompletedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoes_ResponsibleSectorId",
                table: "ChecklistItensAcoes",
                column: "ResponsibleSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoes_ResponsibleSupervisorId",
                table: "ChecklistItensAcoes",
                column: "ResponsibleSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoesHistorico_ChecklistItemActionId",
                table: "ChecklistItensAcoesHistorico",
                column: "ChecklistItemActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensAcoesHistorico_CreatedBySupervisorId",
                table: "ChecklistItensAcoesHistorico",
                column: "CreatedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensTemplate_CategoryId_Order",
                table: "ChecklistItensTemplate",
                columns: new[] { "CategoryId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItensTemplate_SectorId",
                table: "ChecklistItensTemplate",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_EquipmentId",
                table: "Checklists",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_OperatorId",
                table: "Checklists",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_SectorId_EquipmentId_ReferenceDate",
                table: "Checklists",
                columns: new[] { "SectorId", "EquipmentId", "ReferenceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistsEnviados_EquipmentId",
                table: "ChecklistsEnviados",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_CategoryId",
                table: "Equipamentos",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_QrId",
                table: "Equipamentos",
                column: "QrId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_SectorId",
                table: "Equipamentos",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosChecklistMensais_ClosedBySupervisorId",
                table: "FechamentosChecklistMensais",
                column: "ClosedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosChecklistMensais_EquipmentId",
                table: "FechamentosChecklistMensais",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosChecklistMensais_SectorId_EquipmentId_Year_Month",
                table: "FechamentosChecklistMensais",
                columns: new[] { "SectorId", "EquipmentId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosChecklistMensaisChecklists_ChecklistId",
                table: "FechamentosChecklistMensaisChecklists",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosChecklistMensaisChecklists_MonthlyChecklistClosureId_ChecklistId",
                table: "FechamentosChecklistMensaisChecklists",
                columns: new[] { "MonthlyChecklistClosureId", "ChecklistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operadores_Login",
                table: "Operadores",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operadores_SectorId_Registration",
                table: "Operadores",
                columns: new[] { "SectorId", "Registration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Setores_Name",
                table: "Setores",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklistItens_ChecklistId_Order",
                table: "StpAreaChecklistItens",
                columns: new[] { "ChecklistId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklistItens_TemplateItemId",
                table: "StpAreaChecklistItens",
                column: "TemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklists_InspectedSectorId",
                table: "StpAreaChecklists",
                column: "InspectedSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklists_InspectionAreaId",
                table: "StpAreaChecklists",
                column: "InspectionAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklists_InspectorSupervisorId",
                table: "StpAreaChecklists",
                column: "InspectorSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklists_SectorId_ReferenceDate",
                table: "StpAreaChecklists",
                columns: new[] { "SectorId", "ReferenceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklists_TemplateId",
                table: "StpAreaChecklists",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklistTemplateItens_TemplateId_Order",
                table: "StpAreaChecklistTemplateItens",
                columns: new[] { "TemplateId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpAreaChecklistTemplates_SectorId_Code",
                table: "StpAreaChecklistTemplates",
                columns: new[] { "SectorId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpAreasInspecao_ResponsibleSupervisorId",
                table: "StpAreasInspecao",
                column: "ResponsibleSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_StpAreasInspecao_SectorId_Name",
                table: "StpAreasInspecao",
                columns: new[] { "SectorId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosEmpresas_SectorId_Name",
                table: "StpDocumentosEmpresas",
                columns: new[] { "SectorId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosEmpresasArquivos_CompanyId",
                table: "StpDocumentosEmpresasArquivos",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosEmpresasArquivos_UploadedBySupervisorId",
                table: "StpDocumentosEmpresasArquivos",
                column: "UploadedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosFuncionarios_CompanyId_Name",
                table: "StpDocumentosFuncionarios",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosFuncionariosArquivos_EmployeeId",
                table: "StpDocumentosFuncionariosArquivos",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StpDocumentosFuncionariosArquivos_UploadedBySupervisorId",
                table: "StpDocumentosFuncionariosArquivos",
                column: "UploadedBySupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSupervisores_Email",
                table: "UsuariosSupervisores",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSupervisores_Login",
                table: "UsuariosSupervisores",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSupervisores_SectorId",
                table: "UsuariosSupervisores",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSupervisoresModulos_SupervisorUserId_Module",
                table: "UsuariosSupervisoresModulos",
                columns: new[] { "SupervisorUserId", "Module" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItensAcoesHistorico");

            migrationBuilder.DropTable(
                name: "ChecklistsEnviados");

            migrationBuilder.DropTable(
                name: "FechamentosChecklistMensaisChecklists");

            migrationBuilder.DropTable(
                name: "StpAreaChecklistItens");

            migrationBuilder.DropTable(
                name: "StpDocumentosEmpresasArquivos");

            migrationBuilder.DropTable(
                name: "StpDocumentosFuncionariosArquivos");

            migrationBuilder.DropTable(
                name: "UsuariosSupervisoresModulos");

            migrationBuilder.DropTable(
                name: "ChecklistItensAcoes");

            migrationBuilder.DropTable(
                name: "FechamentosChecklistMensais");

            migrationBuilder.DropTable(
                name: "StpAreaChecklistTemplateItens");

            migrationBuilder.DropTable(
                name: "StpAreaChecklists");

            migrationBuilder.DropTable(
                name: "StpDocumentosFuncionarios");

            migrationBuilder.DropTable(
                name: "ChecklistItens");

            migrationBuilder.DropTable(
                name: "StpAreaChecklistTemplates");

            migrationBuilder.DropTable(
                name: "StpAreasInspecao");

            migrationBuilder.DropTable(
                name: "StpDocumentosEmpresas");

            migrationBuilder.DropTable(
                name: "ChecklistItensTemplate");

            migrationBuilder.DropTable(
                name: "Checklists");

            migrationBuilder.DropTable(
                name: "UsuariosSupervisores");

            migrationBuilder.DropTable(
                name: "Equipamentos");

            migrationBuilder.DropTable(
                name: "Operadores");

            migrationBuilder.DropTable(
                name: "CategoriasEquipamento");

            migrationBuilder.DropTable(
                name: "Setores");
        }
    }
}
