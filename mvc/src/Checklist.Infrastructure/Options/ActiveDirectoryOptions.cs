namespace Checklist.Infrastructure.Options;

public class ActiveDirectoryOptions
{
    public const string SectionName = "ActiveDirectory";

    public string Domain { get; set; } = string.Empty;
    public string? Container { get; set; }
}
