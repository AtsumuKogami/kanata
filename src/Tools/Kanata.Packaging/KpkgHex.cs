namespace Kanata.Packaging;

internal static class KpkgHex
{
    public static byte[] DecodeSha256(string value, string fieldName)
    {
        if (value.Length != 64)
        {
            throw new KpkgFormatException($"{fieldName} must be a 64-character SHA-256 hex string.");
        }

        try
        {
            return Convert.FromHexString(value);
        }
        catch (FormatException exception)
        {
            throw new KpkgFormatException($"{fieldName} is not valid hexadecimal: {exception.Message}");
        }
    }

    public static string EncodeSha256(byte[] value)
    {
        if (value.Length != 32)
        {
            throw new KpkgFormatException("SHA-256 hash must be 32 bytes.");
        }

        return Convert.ToHexString(value).ToLowerInvariant();
    }
}
