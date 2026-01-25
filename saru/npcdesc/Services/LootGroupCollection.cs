using NpcDesc.Models;
using NpcDesc.Utilities;

namespace NpcDesc.Services;

public class LootGroupCollection
{
    public Dictionary<int, LootGroup> LootGroups { get; } = new();
    public Dictionary<string, ItemGroup> ItemGroups { get; } = new(StringComparer.OrdinalIgnoreCase);

    public LootGroup? GetLootGroup(int id)
    {
        return LootGroups.TryGetValue(id, out var group) ? group : null;
    }

    public ItemGroup? GetItemGroup(string name)
    {
        return ItemGroups.TryGetValue(name, out var group) ? group : null;
    }

    public void ResolveReferences()
    {
        foreach (var lootGroup in LootGroups.Values)
        {
            ResolveEntriesReferences(lootGroup.Entries);
        }

        foreach (var itemGroup in ItemGroups.Values)
        {
            ResolveEntriesReferences(itemGroup.Entries);
        }
    }

    private void ResolveEntriesReferences(List<LootEntry> entries)
    {
        foreach (var entry in entries)
        {
            switch (entry)
            {
                case RandomGroupEntry randomEntry:
                    randomEntry.ResolvedGroup = GetItemGroup(randomEntry.GroupName);
                    break;
                case IncludeGroupEntry includeEntry:
                    includeEntry.ResolvedGroup = GetItemGroup(includeEntry.GroupName);
                    break;
            }
        }
    }

    public IEnumerable<PossibleLootItem> GetPossibleItems(int lootGroupId)
    {
        var lootGroup = GetLootGroup(lootGroupId);
        if (lootGroup == null)
        {
            yield break;
        }

        foreach (var item in GetPossibleItemsFromEntries(lootGroup.Entries, 100.0, new List<string> { $"lootgroup {lootGroupId}" }))
        {
            yield return item;
        }
    }

    private IEnumerable<PossibleLootItem> GetPossibleItemsFromEntries(
        List<LootEntry> entries, 
        double parentChance, 
        List<string> currentPath,
        HashSet<string>? visited = null)
    {
        visited ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            var effectiveChance = (parentChance * entry.ChancePercent) / 100.0;

            switch (entry)
            {
                case ItemLootEntry itemEntry:
                    yield return new PossibleLootItem
                    {
                        ItemName = itemEntry.ItemName,
                        Amount = itemEntry.Amount,
                        EffectiveChance = effectiveChance,
                        IsStack = itemEntry.IsStack,
                        SourcePath = new List<string>(currentPath)
                    };
                    break;

                case RandomGroupEntry randomEntry when randomEntry.ResolvedGroup != null:
                    if (!visited.Contains(randomEntry.GroupName))
                    {
                        visited.Add(randomEntry.GroupName);
                        var newPath = new List<string>(currentPath) { $"Random -> {randomEntry.GroupName}" };
                        foreach (var item in GetPossibleItemsFromEntries(
                            randomEntry.ResolvedGroup.Entries, 
                            effectiveChance, 
                            newPath,
                            visited))
                        {
                            // For Random, we pick from the group, so items inherit the random entry's amount
                            yield return item with { Amount = randomEntry.Amount };
                        }
                        visited.Remove(randomEntry.GroupName);
                    }
                    break;

                case IncludeGroupEntry includeEntry when includeEntry.ResolvedGroup != null:
                    if (!visited.Contains(includeEntry.GroupName))
                    {
                        visited.Add(includeEntry.GroupName);
                        var newPath = new List<string>(currentPath) { $"Group -> {includeEntry.GroupName}" };
                        foreach (var item in GetPossibleItemsFromEntries(
                            includeEntry.ResolvedGroup.Entries, 
                            effectiveChance, 
                            newPath,
                            visited))
                        {
                            yield return item;
                        }
                        visited.Remove(includeEntry.GroupName);
                    }
                    break;
            }
        }
    }
}
