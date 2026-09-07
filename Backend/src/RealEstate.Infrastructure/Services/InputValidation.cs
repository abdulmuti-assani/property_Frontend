namespace RealEstate.Infrastructure.Services;

/// <summary>
/// Shared, authoritative validation for free-text fields the frontend also checks.
/// Throws <see cref="InvalidOperationException"/> (mapped to HTTP 400) on failure.
/// </summary>
internal static class InputValidation
{
    private static readonly char[] AllowedPhoneChars = { '+', ' ', '-', '(', ')' };

    /// <summary>A person's name must contain letters — pure numbers are rejected.</summary>
    public static string ValidateName(string? name, string label = "Name")
    {
        var trimmed = (name ?? string.Empty).Trim();

        if (trimmed.Length < 2 || trimmed.Count(char.IsLetter) < 2)
            throw new InvalidOperationException($"{label} must be a real name, not numbers.");

        return trimmed;
    }

    /// <summary>
    /// Phone is optional; when supplied it must be digits (7–15 of them) with only
    /// +, spaces, dashes or parentheses as separators. Letters are rejected.
    /// Returns null for an empty value, otherwise the trimmed number.
    /// </summary>
    public static string? ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var trimmed = phone.Trim();
        var digitCount = trimmed.Count(char.IsDigit);
        var onlyAllowed = trimmed.All(c => char.IsDigit(c) || AllowedPhoneChars.Contains(c));

        if (!onlyAllowed || digitCount < 7 || digitCount > 15)
            throw new InvalidOperationException("Enter a valid phone number (7–15 digits, no letters).");

        return trimmed;
    }
}
