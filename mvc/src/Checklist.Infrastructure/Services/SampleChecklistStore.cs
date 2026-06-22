using Checklist.Application.Dtos;

namespace Checklist.Infrastructure.Services;

internal static class SampleChecklistStore
{
    private static readonly object SyncRoot = new();
    private static int _operatorChecklistSequence = 200;

    public static readonly Guid ChecklistId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid EquipmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid SectorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OperatorId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid OperatorCategoryId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    public static readonly Guid OperatorEquipmentId = Guid.Parse("dededede-dede-dede-dede-dededededede");
    public static readonly Guid OperatorEquipmentQrId = Guid.Parse("abababab-abab-abab-abab-abababababab");
    public static readonly Guid NonOkItemPendingId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid NonOkItemInProgressId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid NonOkItemCompletedId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly IReadOnlyList<OperatorEquipmentDto> OperatorEquipmentSeed =
    [
        new OperatorEquipmentDto
        {
            Id = OperatorEquipmentId,
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            QrId = OperatorEquipmentQrId,
            Code = "EMP-061",
            Description = "Empilhadeira eletrica Jungheinrich EFG 320",
            CategoryName = "Eletrica",
            IsActive = true
        },
        new OperatorEquipmentDto
        {
            Id = Guid.Parse("efefefef-efef-efef-efef-efefefefefef"),
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            QrId = Guid.Parse("cdcdcdcd-cdcd-cdcd-cdcd-cdcdcdcdcdcd"),
            Code = "EMP-062",
            Description = "Empilhadeira eletrica Toyota 8FBE18",
            CategoryName = "Eletrica",
            IsActive = true
        }
    ];

    private static readonly IReadOnlyList<OperatorChecklistTemplateItemDto> OperatorChecklistTemplateSeed =
    [
        new OperatorChecklistTemplateItemDto
        {
            Id = Guid.Parse("01010101-0101-0101-0101-010101010101"),
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            Order = 1,
            Description = "Bateria principal",
            Instruction = "Verificar carga e conectores.",
            IsActive = true
        },
        new OperatorChecklistTemplateItemDto
        {
            Id = Guid.Parse("02020202-0202-0202-0202-020202020202"),
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            Order = 2,
            Description = "Freio de estacionamento",
            Instruction = "Validar acionamento completo antes do deslocamento.",
            IsActive = true
        },
        new OperatorChecklistTemplateItemDto
        {
            Id = Guid.Parse("03030303-0303-0303-0303-030303030303"),
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            Order = 3,
            Description = "Buzina e alarme de re",
            Instruction = "Confirmar emissao sonora padrao.",
            IsActive = true
        },
        new OperatorChecklistTemplateItemDto
        {
            Id = Guid.Parse("04040404-0404-0404-0404-040404040404"),
            SectorId = SectorId,
            CategoryId = OperatorCategoryId,
            Order = 4,
            Description = "Garfos e corrente de elevacao",
            Instruction = "Inspecionar deformacoes e desgaste fora do limite.",
            IsActive = true
        }
    ];

    private static readonly IReadOnlyList<DashboardEquipmentStatusDto> DashboardEquipmentSeed =
    [
        new DashboardEquipmentStatusDto
        {
            EquipmentId = EquipmentId,
            EquipmentCode = "EMP-045",
            EquipmentDescription = "Empilhadeira Eletrica Toyota 8FBE20",
            CategoryName = "Eletrica",
            Status = "nok",
            ChecklistId = ChecklistId,
            ChecklistCompletedAtUtc = new DateTime(2026, 05, 24, 14, 30, 00, DateTimeKind.Utc)
        },
        new DashboardEquipmentStatusDto
        {
            EquipmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            EquipmentCode = "EMP-046",
            EquipmentDescription = "Empilhadeira Retratil Crown ESR 5260",
            CategoryName = "Retratil",
            Status = "ok",
            ChecklistId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ChecklistCompletedAtUtc = new DateTime(2026, 05, 24, 13, 10, 00, DateTimeKind.Utc)
        },
        new DashboardEquipmentStatusDto
        {
            EquipmentId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            EquipmentCode = "EMP-052",
            EquipmentDescription = "Paleteira eletrica Yale MP20X",
            CategoryName = "Paleteira",
            Status = "nao-preenchido"
        },
        new DashboardEquipmentStatusDto
        {
            EquipmentId = OperatorEquipmentId,
            EquipmentCode = "EMP-061",
            EquipmentDescription = "Empilhadeira eletrica Jungheinrich EFG 320",
            CategoryName = "Eletrica",
            Status = "nao-preenchido"
        },
        new DashboardEquipmentStatusDto
        {
            EquipmentId = Guid.Parse("efefefef-efef-efef-efef-efefefefefef"),
            EquipmentCode = "EMP-062",
            EquipmentDescription = "Empilhadeira eletrica Toyota 8FBE18",
            CategoryName = "Eletrica",
            Status = "nao-preenchido"
        }
    ];

    private static readonly IReadOnlyList<ChecklistListItemDto> ChecklistSeed =
    [
        new ChecklistListItemDto
        {
            Id = ChecklistId,
            SectorId = SectorId,
            EquipmentCode = "EMP-045",
            EquipmentDescription = "Empilhadeira Eletrica Toyota 8FBE20",
            OperatorName = "Carlos Henrique",
            OperatorRegistration = "OP1001",
            CreatedAt = new DateTime(2026, 05, 24, 14, 30, 00, DateTimeKind.Utc),
            Status = "nok",
            TotalItems = 3,
            ItemsOk = 2,
            ItemsNok = 1
        },
        new ChecklistListItemDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            SectorId = SectorId,
            EquipmentCode = "EMP-046",
            EquipmentDescription = "Empilhadeira Retratil Crown ESR 5260",
            OperatorName = "Marina Souza",
            OperatorRegistration = "OP1021",
            CreatedAt = new DateTime(2026, 05, 24, 13, 10, 00, DateTimeKind.Utc),
            Status = "ok",
            TotalItems = 4,
            ItemsOk = 4,
            ItemsNok = 0
        }
    ];

    private static readonly Dictionary<Guid, ChecklistDetailsDto> ChecklistDetailsSeed = new()
    {
        [ChecklistId] = new ChecklistDetailsDto
        {
            Id = ChecklistId,
            Code = "CHK-TEST-001",
            EquipmentCode = "EMP-045",
            EquipmentDescription = "Empilhadeira Eletrica Toyota 8FBE20",
            OperatorName = "Carlos Henrique",
            SectorName = "Expedicao",
            Status = "Nao conforme",
            CreatedAtUtc = new DateTime(2026, 05, 24, 14, 30, 00, DateTimeKind.Utc),
            Items =
            [
                new ChecklistItemDto
                {
                    Label = "Bateria principal",
                    Status = "Ok",
                    Notes = "Carga dentro da faixa esperada."
                },
                new ChecklistItemDto
                {
                    Label = "Freio de estacionamento",
                    Status = "Nok",
                    Notes = "Ajuste preventivo recomendado para o proximo turno."
                },
                new ChecklistItemDto
                {
                    Label = "Garfo esquerdo",
                    Status = "Ok",
                    Notes = "Sem desgaste fora do padrao."
                }
            ]
        },
        [Guid.Parse("22222222-2222-2222-2222-222222222222")] = new ChecklistDetailsDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "CHK-TEST-002",
            EquipmentCode = "EMP-046",
            EquipmentDescription = "Empilhadeira Retratil Crown ESR 5260",
            OperatorName = "Marina Souza",
            SectorName = "Expedicao",
            Status = "Conforme",
            CreatedAtUtc = new DateTime(2026, 05, 24, 13, 10, 00, DateTimeKind.Utc),
            Items =
            [
                new ChecklistItemDto
                {
                    Label = "Bateria principal",
                    Status = "Ok",
                    Notes = "Sem observacoes."
                },
                new ChecklistItemDto
                {
                    Label = "Freio de estacionamento",
                    Status = "Ok",
                    Notes = "Acionamento dentro do esperado."
                },
                new ChecklistItemDto
                {
                    Label = "Buzina e alarme de re",
                    Status = "Ok",
                    Notes = "Sinalizacao normal."
                },
                new ChecklistItemDto
                {
                    Label = "Garfos e corrente de elevacao",
                    Status = "Ok",
                    Notes = "Sem desgaste fora do limite."
                }
            ]
        }
    };

    private static readonly IReadOnlyList<NonOkPanelItemDto> NonOkSeed =
    [
        new NonOkPanelItemDto
        {
            ChecklistId = ChecklistId,
            ChecklistItemId = NonOkItemPendingId,
            ChecklistCompletedAt = new DateTime(2026, 05, 24, 14, 30, 00, DateTimeKind.Utc),
            SourceSectorId = SectorId,
            SourceSectorName = "Expedicao",
            EquipmentCode = "EMP-045",
            EquipmentDescription = "Empilhadeira Eletrica Toyota 8FBE20",
            OperatorName = "Carlos Henrique",
            OperatorRegistration = "OP1001",
            Order = 2,
            Description = "Freio de estacionamento com resposta inconsistente.",
            Instruction = "Validar ajuste mecanico antes do proximo turno.",
            Notes = "Travamento parcial durante o teste operacional.",
            WorkflowStatus = "pending-approval",
            History =
            [
                new NonOkHistoryEntryDto
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                    Title = "Ocorrencia registrada",
                    Description = "Carlos Henrique registrou o item como non-compliant durante o checklist.",
                    CreatedAt = new DateTime(2026, 05, 24, 14, 30, 00, DateTimeKind.Utc),
                    CreatedByDisplayName = "Carlos Henrique"
                }
            ]
        },
        new NonOkPanelItemDto
        {
            ChecklistId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ChecklistItemId = NonOkItemInProgressId,
            ChecklistCompletedAt = new DateTime(2026, 05, 24, 13, 10, 00, DateTimeKind.Utc),
            SourceSectorId = SectorId,
            SourceSectorName = "Expedicao",
            EquipmentCode = "EMP-046",
            EquipmentDescription = "Empilhadeira Retratil Crown ESR 5260",
            OperatorName = "Marina Souza",
            OperatorRegistration = "OP1021",
            Order = 4,
            Description = "Alarme de re sem emissao sonora padrao.",
            Instruction = "Confirmar circuito e sirene.",
            Notes = "Sem alarme na primeira manobra do turno.",
            WorkflowStatus = "in-progress",
            ResponsibleSupervisorId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ResponsibleFullName = "Luciana Ramos",
            ResponsibleSectorId = SectorId,
            ResponsibleSectorName = "Expedicao",
            ResponsibleNotes = "Peca separada para substituicao.",
            PlannedCompletionDate = new DateTime(2026, 05, 26, 0, 0, 0, DateTimeKind.Utc),
            CompletionPercentage = 60,
            ApprovedAt = new DateTime(2026, 05, 24, 16, 00, 00, DateTimeKind.Utc),
            ApprovedByFullName = "Rafael Costa",
            History =
            [
                new NonOkHistoryEntryDto
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                    Title = "Tratativa atribuida",
                    Description = "Luciana Ramos assumiu a analise do alarme de re com previsao para 26/05/2026.",
                    CreatedAt = new DateTime(2026, 05, 24, 16, 00, 00, DateTimeKind.Utc),
                    CreatedByDisplayName = "Rafael Costa"
                }
            ]
        },
        new NonOkPanelItemDto
        {
            ChecklistId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            ChecklistItemId = NonOkItemCompletedId,
            ChecklistCompletedAt = new DateTime(2026, 05, 23, 9, 15, 00, DateTimeKind.Utc),
            SourceSectorId = SectorId,
            SourceSectorName = "Expedicao",
            EquipmentCode = "EMP-052",
            EquipmentDescription = "Paleteira eletrica Yale MP20X",
            OperatorName = "Aline Moraes",
            OperatorRegistration = "OP1038",
            Order = 1,
            Description = "Roda de apoio com desgaste acima do limite.",
            Instruction = "Substituir conjunto antes de liberar o equipamento.",
            Notes = "Vibracao no deslocamento vazio.",
            WorkflowStatus = "completed",
            ResponsibleSupervisorId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            ResponsibleFullName = "Joao Henrique",
            ResponsibleSectorId = SectorId,
            ResponsibleSectorName = "Expedicao",
            ResponsibleNotes = "Conjunto substituido e validado em teste curto.",
            PlannedCompletionDate = new DateTime(2026, 05, 24, 0, 0, 0, DateTimeKind.Utc),
            CompletionPercentage = 100,
            ApprovedAt = new DateTime(2026, 05, 23, 11, 00, 00, DateTimeKind.Utc),
            ApprovedByFullName = "Rafael Costa",
            WorkflowCompletedAt = new DateTime(2026, 05, 24, 10, 20, 00, DateTimeKind.Utc),
            CompletedByFullName = "Joao Henrique",
            History =
            [
                new NonOkHistoryEntryDto
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000003"),
                    Title = "Tratativa atribuida",
                    Description = "Joao Henrique recebeu a tratativa para substituicao da roda de apoio.",
                    CreatedAt = new DateTime(2026, 05, 23, 11, 00, 00, DateTimeKind.Utc),
                    CreatedByDisplayName = "Rafael Costa"
                },
                new NonOkHistoryEntryDto
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000004"),
                    Title = "Tratativa concluida",
                    Description = "Conjunto substituido e validado em teste curto sem nova vibracao.",
                    CreatedAt = new DateTime(2026, 05, 24, 10, 20, 00, DateTimeKind.Utc),
                    CreatedByDisplayName = "Joao Henrique"
                }
            ]
        }
    ];

    private static readonly IReadOnlyList<NonOkResponsibleOptionDto> NonOkResponsibleOptionSeed =
    [
        new NonOkResponsibleOptionDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "Luciana Ramos",
            Login = "luciana.ramos",
            SectorId = SectorId,
            SectorName = "Expedicao"
        },
        new NonOkResponsibleOptionDto
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            FullName = "Joao Henrique",
            Login = "joao.henrique",
            SectorId = SectorId,
            SectorName = "Expedicao"
        }
    ];

    private static readonly List<DashboardEquipmentStatusDto> DashboardEquipmentState = DashboardEquipmentSeed.ToList();
    private static readonly List<ChecklistListItemDto> ChecklistState = ChecklistSeed.ToList();
    private static readonly Dictionary<Guid, ChecklistDetailsDto> ChecklistDetailsState = new(ChecklistDetailsSeed);

    public static List<NonOkPanelItemDto> NonOkItemState { get; } = NonOkSeed.ToList();

    public static OperatorSessionDto OperatorSession => new()
    {
        Id = OperatorId,
        SectorId = SectorId,
        Name = "Gabriel Candido",
        Registration = "0708813",
        Login = "GabrielCandido",
        SectorName = "SCE - Expedição",
        ForceChangePassword = false
    };

    public static IReadOnlyList<OperatorEquipmentDto> OperatorEquipments => OperatorEquipmentSeed;
    public static IReadOnlyList<OperatorChecklistTemplateItemDto> OperatorChecklistTemplates => OperatorChecklistTemplateSeed;
    public static IReadOnlyList<NonOkResponsibleOptionDto> NonOkResponsibleOptions => NonOkResponsibleOptionSeed;

    public static IReadOnlyList<DashboardEquipmentStatusDto> GetDashboardEquipments()
    {
        lock (SyncRoot)
        {
            return DashboardEquipmentState
                .OrderBy(entry => entry.EquipmentCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public static IReadOnlyList<ChecklistListItemDto> GetChecklists()
    {
        lock (SyncRoot)
        {
            return ChecklistState
                .OrderByDescending(entry => entry.CreatedAt)
                .ToList();
        }
    }

    public static ChecklistDetailsDto? GetChecklist(Guid checklistId)
    {
        lock (SyncRoot)
        {
            return ChecklistDetailsState.TryGetValue(checklistId, out var checklist)
                ? checklist
                : null;
        }
    }

    public static bool HasChecklistForEquipmentOnDate(string equipmentCode, DateTime dateUtc)
    {
        lock (SyncRoot)
        {
            return ChecklistState.Any(entry =>
                string.Equals(entry.EquipmentCode, equipmentCode, StringComparison.OrdinalIgnoreCase)
                && entry.CreatedAt.Date == dateUtc.Date);
        }
    }

    public static OperatorChecklistResultDto RegisterOperatorChecklist(
        OperatorEquipmentDto equipment,
        OperatorSessionDto operador,
        IReadOnlyList<OperatorChecklistTemplateItemDto> templates,
        OperatorChecklistSubmissionDto request,
        DateTime submittedAtUtc)
    {
        lock (SyncRoot)
        {
            var approved = request.Items.All(item => IsApprovedStatus(item.Status));
            var checklistId = Guid.NewGuid();
            var checklistCode = $"CHK-OP-{++_operatorChecklistSequence:D3}";
            var itemLookup = request.Items.ToDictionary(item => item.TemplateId);

            var checklistItems = templates
                .OrderBy(template => template.Order)
                .Select(template =>
                {
                    var submittedItem = itemLookup[template.Id];
                    return new ChecklistItemDto
                    {
                        Label = template.Description,
                        Status = NormalizeChecklistItemStatus(submittedItem.Status),
                        Notes = NormalizeOptionalText(submittedItem.Notes)
                    };
                })
                .ToList();

            var checklistDetails = new ChecklistDetailsDto
            {
                Id = checklistId,
                Code = checklistCode,
                EquipmentCode = equipment.Code,
                EquipmentDescription = equipment.Description,
                OperatorName = operador.Name,
                SectorName = operador.SectorName,
                Status = approved ? "Conforme" : "Nao conforme",
                CreatedAtUtc = submittedAtUtc,
                Items = checklistItems
            };

            var checklistListItem = new ChecklistListItemDto
            {
                Id = checklistId,
                SectorId = equipment.SectorId,
                EquipmentCode = equipment.Code,
                EquipmentDescription = equipment.Description,
                OperatorName = operador.Name,
                OperatorRegistration = operador.Registration,
                CreatedAt = submittedAtUtc,
                Status = approved ? "ok" : "nok",
                TotalItems = request.Items.Count,
                ItemsOk = request.Items.Count(item => string.Equals(item.Status, "OK", StringComparison.OrdinalIgnoreCase)),
                ItemsNok = request.Items.Count(item => string.Equals(item.Status, "NOK", StringComparison.OrdinalIgnoreCase))
            };

            ChecklistDetailsState[checklistId] = checklistDetails;
            ChecklistState.Insert(0, checklistListItem);

            UpsertDashboardEquipment(new DashboardEquipmentStatusDto
            {
                EquipmentId = equipment.Id,
                EquipmentCode = equipment.Code,
                EquipmentDescription = equipment.Description,
                CategoryName = equipment.CategoryName,
                Status = approved ? "ok" : "nok",
                ChecklistId = checklistId,
                ChecklistCompletedAtUtc = submittedAtUtc
            });

            foreach (var nokItem in templates
                .OrderBy(template => template.Order)
                .Select(template => new { Template = template, Submitted = itemLookup[template.Id] })
                .Where(entry => string.Equals(entry.Submitted.Status, "NOK", StringComparison.OrdinalIgnoreCase)))
            {
                NonOkItemState.Insert(0, new NonOkPanelItemDto
                {
                    ChecklistId = checklistId,
                    ChecklistItemId = Guid.NewGuid(),
                    ChecklistCompletedAt = submittedAtUtc,
                    SourceSectorId = equipment.SectorId,
                    SourceSectorName = operador.SectorName,
                    EquipmentCode = equipment.Code,
                    EquipmentDescription = equipment.Description,
                    OperatorName = operador.Name,
                    OperatorRegistration = operador.Registration,
                    Order = nokItem.Template.Order,
                    Description = nokItem.Template.Description,
                    Instruction = nokItem.Template.Instruction,
                    Notes = NormalizeOptionalText(nokItem.Submitted.Notes),
                    NokImageBase64 = NormalizeOptionalText(nokItem.Submitted.NokImageBase64),
                    NokImageFileName = NormalizeOptionalText(nokItem.Submitted.NokImageFileName),
                    NokImageMimeType = NormalizeOptionalText(nokItem.Submitted.NokImageMimeType),
                    WorkflowStatus = "pending-approval",
                    History =
                    [
                        new NonOkHistoryEntryDto
                        {
                            Id = Guid.NewGuid(),
                            Title = "Ocorrencia registrada",
                            Description = $"{operador.Name} registrou o item como non-compliant durante o checklist operacional.",
                            CreatedAt = submittedAtUtc,
                            CreatedByDisplayName = operador.Name
                        }
                    ]
                });
            }

            return new OperatorChecklistResultDto
            {
                Id = checklistId,
                SectorId = equipment.SectorId,
                EquipmentId = equipment.Id,
                EquipmentCode = equipment.Code,
                OperatorId = operador.Id,
                OperatorName = operador.Name,
                CompletedAtUtc = submittedAtUtc,
                IsApproved = approved,
                Status = approved ? "Pending" : "Reprovado"
            };
        }
    }

    private static void UpsertDashboardEquipment(DashboardEquipmentStatusDto updatedEquipment)
    {
        var index = DashboardEquipmentState.FindIndex(entry => entry.EquipmentId == updatedEquipment.EquipmentId);
        if (index >= 0)
        {
            DashboardEquipmentState[index] = updatedEquipment;
            return;
        }

        DashboardEquipmentState.Add(updatedEquipment);
    }

    private static bool IsApprovedStatus(string status)
    {
        return string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "NA", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeChecklistItemStatus(string status)
    {
        if (string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return "Ok";
        }

        if (string.Equals(status, "NOK", StringComparison.OrdinalIgnoreCase))
        {
            return "Nok";
        }

        if (string.Equals(status, "NA", StringComparison.OrdinalIgnoreCase))
        {
            return "NA";
        }

        return status;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}


