using NpcDesc.Utilities;

namespace NpcDesc.Models;

public record PossibleLootItem
{
    public string ItemName { get; set; } = string.Empty;
    public DiceRoll Amount { get; set; } = DiceRoll.One;
    public double EffectiveChance { get; set; } = 100.0;
    public bool IsStack { get; set; }
    public List<string> SourcePath { get; set; } = new();
}
