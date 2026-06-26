using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Checklist.Infrastructure.Identity;

public static class SupervisorLoginNormalizer
{
    private static readonly Regex NonAlphaNumericRegex = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var withoutAccents = builder.ToString().Normalize(NormalizationForm.FormC);
        return NonAlphaNumericRegex.Replace(withoutAccents, string.Empty).Trim();
    }
}
