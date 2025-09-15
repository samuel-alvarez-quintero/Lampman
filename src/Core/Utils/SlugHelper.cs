using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Lampman.Core.Utils;

public static class SlugHelper
{
    public static string GenerateSlug(string text, bool allowDots = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // 1. Convert to lowercase and remove leading/trailing whitespace
        text = text.ToLowerInvariant().Trim();

        // 2. Remove diacritics/accents
        text = RemoveDiacritics(text);

        // 3. Replace spaces and invalid characters with hyphens
        text = Regex.Replace(text, allowDots ? @"[^a-z0-9\s-\.]" : @"[^a-z0-9\s-]", ""); // Remove non-alphanumeric except space and hyphen
        text = Regex.Replace(text, @"\s+", "-");        // Replace spaces with single hyphen
        text = Regex.Replace(text, @"-+", "-");         // Replace multiple hyphens with single hyphen

        // 4. Trim leading/trailing hyphens
        text = text.Trim('-');

        return text;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}