namespace Checklist.Mvc.ViewModels;

public class ChecklistListItemViewModel
{
    public Guid Id { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string OperatorRegistration { get; init; }
    public required string CreatedAtDisplay { get; init; }
    public required string StatusKey { get; init; }
    public required string StatusLabel { get; init; }
    public required string StatusCssClass { get; init; }
    public int TotalItems { get; init; }
    public int ItemsOk { get; init; }
    public int ItemsNok { get; init; }
}
