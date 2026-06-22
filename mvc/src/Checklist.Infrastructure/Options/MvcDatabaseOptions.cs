namespace Checklist.Infrastructure.Options;

public class MvcDatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string? Default { get; set; }
    public string? AppDbConnectionString { get; set; }
}
