namespace Checklist.Infrastructure.Identity;

public static class OperatorLoginNormalizer
{
    public static string Normalize(string? value)
    {
        return SupervisorLoginNormalizer.Normalize(value ?? string.Empty);
    }
}
