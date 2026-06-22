namespace Checklist.Mvc.ViewModels;

public class OperatorEquipmentSearchItemViewModel
{
    public Guid Id { get; init; }
    public Guid QrId { get; init; }
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required string CategoryName { get; init; }
}
