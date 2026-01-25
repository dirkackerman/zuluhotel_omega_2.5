using NpcDesc.Utilities;

namespace NpcDesc.Models;

public abstract class LootEntry
{
    public LootEntryType EntryType { get; init; }
    public DiceRoll Amount { get; set; } = DiceRoll.One;
    public int ChancePercent { get; set; } = 100;
}

public class ItemLootEntry : LootEntry
{
    public string ItemName { get; set; } = string.Empty;
    public int? Color { get; set; }
    public bool IsStack { get; set; }
    public bool IsUnique { get; set; }
}

public class RandomGroupEntry : LootEntry
{
    public string GroupName { get; set; } = string.Empty;
    public ItemGroup? ResolvedGroup { get; set; }
}

public class IncludeGroupEntry : LootEntry
{
    public string GroupName { get; set; } = string.Empty;
    public ItemGroup? ResolvedGroup { get; set; }
}
