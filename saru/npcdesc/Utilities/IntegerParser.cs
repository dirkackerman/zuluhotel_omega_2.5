using System.Globalization;

namespace NpcDesc.Utilities;

public static class IntegerParser
{
    public static int Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("Cannot parse empty value as integer");
        }

        value = value.Trim();

        // Handle hex values (0x prefix)
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.Parse(value[2..], NumberStyles.HexNumber);
        }

        return int.Parse(value);
    }

    public static bool TryParse(string value, out int result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();

        // Handle hex values (0x prefix)
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(value[2..], NumberStyles.HexNumber, null, out result);
        }

        return int.TryParse(value, out result);
    }
}
