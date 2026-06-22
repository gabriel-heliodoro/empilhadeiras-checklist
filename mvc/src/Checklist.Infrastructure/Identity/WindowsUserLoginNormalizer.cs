using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Checklist.Infrastructure.Identity;

internal static class WindowsUserLoginNormalizer
{
    private static readonly Regex NonAlphaNumericRegex = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static IReadOnlyList<string> BuildCandidates(string? identityName)
    {
        if (string.IsNullOrWhiteSpace(identityName))
        {
            return [];
        }

        var raw = identityName.Trim();
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            raw
        };

        var slashIndex = raw.LastIndexOf('\\');
        if (slashIndex >= 0 && slashIndex < raw.Length - 1)
        {
            values.Add(raw[(slashIndex + 1)..]);
        }

        var atIndex = raw.IndexOf('@');
        if (atIndex > 0)
        {
            values.Add(raw[..atIndex]);
        }

        foreach (var value in values.ToArray())
        {
            var normalized = NormalizeToLogin(value);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                values.Add(normalized);
            }
        }

        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeToLogin(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var withoutAccents = builder.ToString().Normalize(NormalizationForm.FormC);
        return NonAlphaNumericRegex.Replace(withoutAccents, string.Empty);
    }
}
