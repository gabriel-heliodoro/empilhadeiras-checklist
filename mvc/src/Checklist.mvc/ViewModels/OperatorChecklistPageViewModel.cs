namespace Checklist.Mvc.ViewModels;

public class OperatorChecklistPageViewModel
{
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentDescription { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid EquipmentQrId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string OperatorRegistration { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public string? GeneralNotes { get; set; }
    public string SignatureBase64 { get; set; } = string.Empty;
    public List<OperatorChecklistItemFormViewModel> Items { get; set; } = [];
}
