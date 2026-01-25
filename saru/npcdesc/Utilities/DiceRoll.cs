using System.Text.RegularExpressions;

namespace NpcDesc.Utilities;

public partial class DiceRoll
{
    public int NumDice { get; init; } = 1;
    public int DiceSides { get; init; } = 1;
    public int Bonus { get; init; } = 0;

    public static DiceRoll One => new() { NumDice = 1, DiceSides = 1, Bonus = 0 };

    // Pattern: "2d6+3", "1d100", "5", "10d5+10"
    [GeneratedRegex(@"^(\d+)(?:d(\d+))?(?:\+(\d+))?$", RegexOptions.IgnoreCase)]
    private static partial Regex DicePattern();

    public static DiceRoll Parse(string notation)
    {
        if (string.IsNullOrWhiteSpace(notation))
        {
            return One;
        }

        notation = notation.Trim();
        var match = DicePattern().Match(notation);

        if (!match.Success)
        {
            // Try to parse as simple integer
            if (int.TryParse(notation, out var simpleValue))
            {
                return new DiceRoll { NumDice = simpleValue, DiceSides = 1, Bonus = 0 };
            }
            throw new FormatException($"Invalid dice notation: {notation}");
        }

        var numDice = int.Parse(match.Groups[1].Value);
        var diceSides = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
        var bonus = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        return new DiceRoll
        {
            NumDice = numDice,
            DiceSides = diceSides,
            Bonus = bonus
        };
    }

    public static bool TryParse(string notation, out DiceRoll result)
    {
        try
        {
            result = Parse(notation);
            return true;
        }
        catch
        {
            result = One;
            return false;
        }
    }

    public int MinValue => NumDice + Bonus;

    public int MaxValue => (NumDice * DiceSides) + Bonus;

    public int Roll(Random? rng = null)
    {
        rng ??= Random.Shared;
        var total = Bonus;
        for (var i = 0; i < NumDice; i++)
        {
            total += rng.Next(1, DiceSides + 1);
        }
        return total;
    }

    public override string ToString()
    {
        if (DiceSides == 1)
        {
            return (NumDice + Bonus).ToString();
        }

        var diceStr = $"{NumDice}d{DiceSides}";
        return Bonus == 0 ? diceStr : $"{diceStr}+{Bonus}";
    }
}
