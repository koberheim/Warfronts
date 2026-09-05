using System;
using System.Text.RegularExpressions;

namespace FrontsOfWar.Map.Authoring;

// Stable authoring IDs are persisted references, never display names or
// array indexes. Generation is editor-only and therefore not gameplay RNG.
public static partial class MapObjectId
{
    public const int MaximumLength = 64;

    [GeneratedRegex("^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")]
    private static partial Regex ValidPattern();

    public static string New(string prefix)
    {
        string normalizedPrefix = NormalizePrefix(prefix);
        return $"{normalizedPrefix}_{Guid.NewGuid():N}"[..Math.Min(MaximumLength, normalizedPrefix.Length + 13)];
    }

    public static bool IsValid(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length <= MaximumLength
            && ValidPattern().IsMatch(value);
    }

    public static string NormalizePrefix(string prefix)
    {
        string source = string.IsNullOrWhiteSpace(prefix) ? "object" : prefix.Trim().ToLowerInvariant();
        var characters = new char[source.Length];
        int length = 0;
        bool previousUnderscore = false;
        foreach (char character in source)
        {
            bool valid = character is >= 'a' and <= 'z' or >= '0' and <= '9';
            char next = valid ? character : '_';
            if (next == '_' && previousUnderscore) continue;
            characters[length++] = next;
            previousUnderscore = next == '_';
        }

        string normalized = new string(characters, 0, length).Trim('_');
        if (normalized.Length == 0 || normalized[0] is < 'a' or > 'z') normalized = $"object_{normalized}".TrimEnd('_');
        int maxPrefixLength = MaximumLength - 13;
        return normalized.Length <= maxPrefixLength ? normalized : normalized[..maxPrefixLength].TrimEnd('_');
    }
}
