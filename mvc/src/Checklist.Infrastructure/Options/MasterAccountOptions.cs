namespace Checklist.Infrastructure.Options;

public class MasterAccountOptions
{
    public const string SectionName = "MasterAccount";

    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string SectorName { get; set; } = "Administracao";
}
