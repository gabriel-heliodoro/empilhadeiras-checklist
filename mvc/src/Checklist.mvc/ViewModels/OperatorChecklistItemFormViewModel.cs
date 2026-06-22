namespace Checklist.Mvc.ViewModels;

public class OperatorChecklistItemFormViewModel
{
    public Guid TemplateId { get; set; }
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public string Status { get; set; } = "NotChecked";
    public string? Notes { get; set; }
    public string? NokImageBase64 { get; set; }
    public string? NokImageFileName { get; set; }
    public string? NokImageMimeType { get; set; }
}
